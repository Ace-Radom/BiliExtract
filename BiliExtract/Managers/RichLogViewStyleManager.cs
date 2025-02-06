using BiliExtract.Extensions;
using BiliExtract.Lib;
using BiliExtract.Lib.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace BiliExtract.Managers;

public class RichLogViewStyleManager
{
    private readonly XmlLanguage _language = XmlLanguage.GetLanguage("en-us");
    private readonly ApplicationSettings _settings = IoCContainer.Resolve<ApplicationSettings>();
    private readonly TextStyleSettings _styleSettings = IoCContainer.Resolve<TextStyleSettings>();
    private readonly ThemeManager _themeManager = IoCContainer.Resolve<ThemeManager>();

    private const byte S_T_NORMAL = 0;
    private const byte S_T_NUMBER = 1;
    private const byte S_T_STRING_SQ = 2;
    private const byte S_T_STRING_DQ = 3;

    private FontFamily[] _availableFonts = [];

    public FontFamily[] AvailableFonts => _availableFonts;

    public FontFamily? DefaultFont => GetFontFamilyByName(_themeManager.IsDarkMode() ? RichLogViewStyle.DarkThemeDefault.Font : RichLogViewStyle.LightThemeDefault.Font);
    public FontFamily? Font
    {
        get
        {
            var font = GetFontFamilyByName(Style.Font);
            if (font is not null)
            {
                return font;
            }
            return DefaultFont;
        }
    }

    public RichLogViewStyle Style
    {
        get
        {
            if (_settings.Data.RichLogViewStyleSource == RichLogViewStyleSource.Custom)
            {
                if (_themeManager.IsDarkMode())
                {
                    return _styleSettings.Data.RichLogViewStyleDark;
                }
                else
                {
                    return _styleSettings.Data.RichLogViewStyleLight;
                }
            }
            else
            {
                if (_themeManager.IsDarkMode())
                {
                    return RichLogViewStyle.DarkThemeDefault;
                }
                else
                {
                    return RichLogViewStyle.LightThemeDefault;
                }
            }
        }
    }

    public event EventHandler? StyleChanged;

    public RichLogViewStyleManager()
    {
        RefreshAvailableSystemFontList();
        _themeManager.ThemeApplied += (_, _) => RaiseStyleChanged();
        return;
    }

    public bool CheckFontAvailableByName(string name) => _availableFonts.Any(f => f.FamilyNames.HasName(_language, name));

    public FontFamily? GetFontFamilyByName(string name) => _availableFonts.Where(f => f.FamilyNames.HasName(_language, name)).FirstOrDefault();

    public Paragraph[] ParseLogMessages(string[] lines)
    {
        var style = Style;

        var paragraphs = new List<Paragraph>();
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            var runs = new List<Run>();
            if (line[0] == '[')
            {
                int bpos = 0;
                int epos = 0;

                runs.Add(new Run().SetTextAndStyle("[", style.NormalTextColor));
                runs.Add(new Run().SetTextAndStyle(line[1..5], style.TimeYMDColor));
                runs.Add(new Run().SetTextAndStyle("/", style.NormalTextColor));
                runs.Add(new Run().SetTextAndStyle(line[6..8], style.TimeYMDColor));
                runs.Add(new Run().SetTextAndStyle("/", style.NormalTextColor));
                runs.Add(new Run().SetTextAndStyle(line[9..11], style.TimeYMDColor));
                runs.Add(new Run().SetTextAndStyle(line[11..24], style.TimeHMSColor));
                runs.Add(new Run().SetTextAndStyle("]", style.NormalTextColor));
                // time

                epos = line.IndexOf(']', 25);
                runs.Add(new Run().SetTextAndStyle(" [", style.NormalTextColor));
                runs.Add(new Run().SetTextAndStyle(line[27..epos], style.NumberColor));
                runs.Add(new Run().SetTextAndStyle("] ", style.NormalTextColor));
                // thread ID

                bpos = epos + 2;
                epos = line.IndexOf(':', epos + 1);
                var level = line[bpos..epos];
                runs.Add(new Run().SetTextAndStyle(level, GetLogLevelColorFromLevelName(level, style), GetLogLevelStyleFromLevelName(level, style)));
                runs.Add(new Run().SetTextAndStyle(":", style.NormalTextColor));
                // log level

                bpos = epos + 1;
                epos = line.LastIndexOf('[') - 1;
                runs.AddRange(ParseMessage(line[bpos..epos], style));
                // msg

                bpos = epos + 2;
                epos = line.LastIndexOf('#');
                runs.Add(new Run().SetTextAndStyle(" [", style.NormalTextColor));
                runs.Add(new Run().SetTextAndStyle(line[bpos..epos], style.FilenameColor));
                runs.Add(new Run().SetTextAndStyle("#", style.NormalTextColor));
                bpos = epos + 1;
                epos = line.LastIndexOf(':');
                runs.Add(new Run().SetTextAndStyle(line[bpos..epos], style.NumberColor));
                runs.Add(new Run().SetTextAndStyle(":", style.NormalTextColor));
                bpos = epos + 1;
                epos = line.LastIndexOf(']');
                runs.Add(new Run().SetTextAndStyle(line[bpos..epos], style.NormalTextColor));
                runs.Add(new Run().SetTextAndStyle("]", style.NormalTextColor));
                // file, line, function
            } // normal log msg
            else if (line[0] == '=')
            {
                runs.Add(new Run().SetTextAndStyle("=== ", style.NormalTextColor));
                runs.Add(new Run().SetTextAndStyle(line[4..(line.Length - 5)], style.ExceptionTextColor, style.ExceptionTextStyle));
                runs.Add(new Run().SetTextAndStyle(" ===", style.NormalTextColor));
            } // exception tag
            else if (line[0] == ' ' && line.IndexOf("at") == 3)
            {
                runs.Add(new Run().SetTextAndStyle("  ", style.NormalTextColor));
                runs.Add(new Run().SetTextAndStyle(line[..(line.Length - 1)], style.ExceptionTextColor, style.ExceptionTextStyle));
            } // traceback
            else
            {
                int epos = line.IndexOf(':');
                if (epos != -1)
                {
                    runs.Add(new Run().SetTextAndStyle(line[..epos], style.ExceptionTextColor, style.ExceptionTextStyle));
                    runs.AddRange(ParseMessage(line[epos..(line.Length - 1)], style));
                }
                else
                {
                    runs.AddRange(ParseMessage(line[..(line.Length - 1)], style));
                }
            } // exception msg

            var paragraph = new Paragraph();
            paragraph.Inlines.AddRange(runs.ToArray());
            paragraph.Margin = new Thickness(0);
            paragraphs.Add(paragraph);
        }

        return paragraphs.ToArray();
    }

    public void RaiseStyleChanged()
    {
        StyleChanged?.Invoke(this, EventArgs.Empty);
        return;
    }

    private static RGBColor GetLogLevelColorFromLevelName(string levelName, RichLogViewStyle style) => levelName switch
    {
        "Debug" => style.LogLevelDebugColor,
        "Info" => style.LogLevelInfoColor,
        "Warning" => style.LogLevelWarningColor,
        "Error" => style.LogLevelErrorColor,
        _ => style.NormalTextColor
    };

    private static TextStyle GetLogLevelStyleFromLevelName(string levelName, RichLogViewStyle style) => levelName switch
    {
        "Debug" => style.LogLevelDebugStyle,
        "Info" => style.LogLevelInfoStyle,
        "Warning" => style.LogLevelWarningStyle,
        "Error" => style.LogLevelErrorStyle,
        _ => TextStyle.Normal
    };

    private static Run[] ParseMessage(string message, RichLogViewStyle style)
    {
        var runs = new List<Run>();
        var buf = new StringBuilder();
        byte style_tag = S_T_NORMAL;
        for (int i = 0; i < message.Length; i++)
        {
            var c = message[i];
            byte tag;
            if (c == '\'')
            {
                if (style_tag != S_T_STRING_SQ && message.IndexOf('\'', i + 1) == -1)
                {
                    tag = S_T_NORMAL;
                }
                else
                {
                    tag = S_T_STRING_SQ;
                }
            }
            else if (c == '"')
            {
                if (style_tag != S_T_STRING_DQ && message.IndexOf('"', i + 1) == -1)
                {
                    tag = S_T_NORMAL;
                }
                else
                {
                    tag = S_T_STRING_DQ;
                }
            }
            else if (char.IsDigit(c))
            {
                tag = S_T_NUMBER;
            }
            else
            {
                tag = S_T_NORMAL;
            }
            if (style_tag == tag)
            {
                buf.Append(c);
                if (style_tag == S_T_STRING_SQ || style_tag == S_T_STRING_DQ)
                {
                    runs.Add(new Run().SetTextAndStyle(buf.ToString(), style.StringColor));
                    style_tag = S_T_NORMAL;
                    buf.Clear();
                }
            }
            else if (buf.Length == 0)
            {
                buf.Append(c);
                style_tag = tag;
            }
            else if (style_tag == S_T_STRING_SQ || style_tag == S_T_STRING_DQ)
            {
                buf.Append(c);
            }
            else
            {
                switch (style_tag)
                {
                    case S_T_NORMAL: runs.Add(new Run().SetTextAndStyle(buf.ToString(), style.NormalTextColor)); break;
                    case S_T_NUMBER: runs.Add(new Run().SetTextAndStyle(buf.ToString(), style.NumberColor)); break;
                    default: runs.Add(new Run().SetTextAndStyle(buf.ToString(), style.NormalTextColor)); break;
                }
                buf.Clear();
                buf.Append(c);
                style_tag = tag;
            }
        }
        if (buf.Length > 0)
        {
            switch (style_tag)
            {
                case S_T_NORMAL: runs.Add(new Run().SetTextAndStyle(buf.ToString(), style.NormalTextColor)); break;
                case S_T_NUMBER: runs.Add(new Run().SetTextAndStyle(buf.ToString(), style.NumberColor)); break;
                case S_T_STRING_SQ: runs.Add(new Run().SetTextAndStyle(buf.ToString(), style.StringColor)); break;
                case S_T_STRING_DQ: runs.Add(new Run().SetTextAndStyle(buf.ToString(), style.StringColor)); break;
                default: runs.Add(new Run().SetTextAndStyle(buf.ToString(), style.NormalTextColor)); break;
            }
        }

        return runs.ToArray();
    }

    private void RefreshAvailableSystemFontList()
    {
        var availableSystemFonts = new List<FontFamily>();
        foreach (var font in Fonts.SystemFontFamilies)
        {
            var fontDictionary = font.FamilyNames;
            if (fontDictionary.TryGetValue(_language, out _))
            {
                availableSystemFonts.Add(font);
            }
        }
        availableSystemFonts.Sort((x, y) => string.Compare(x.FamilyNames[_language], y.FamilyNames[_language], StringComparison.OrdinalIgnoreCase));
        _availableFonts = availableSystemFonts.ToArray();
        return;
    }
}
