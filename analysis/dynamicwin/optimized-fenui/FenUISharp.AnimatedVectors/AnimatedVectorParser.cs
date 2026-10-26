using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using FenUISharp.Logging;
using FenUISharp.Mathematics;
using SkiaSharp;

namespace FenUISharp.AnimatedVectors;

public class AnimatedVectorParser
{
	public static AnimatedVector ParseFAV(Stream inputStream)
	{
		if (inputStream == null)
		{
			throw new Exception("Cannot parse null stream.");
		}
		using StreamReader streamReader = new StreamReader(inputStream);
		return ParseFAV(streamReader.ReadToEnd());
	}

	public static AnimatedVector ParseFAV(string rawFavXML)
	{
		XElement? obj = XDocument.Parse(rawFavXML)?.Root ?? throw new Exception("Input FAV is invalid");
		XElement xElement = obj?.Element("symbol") ?? throw new Exception("Input FAV is invalid");
		XElement xElement2 = obj?.Element("animations") ?? throw new Exception("Input FAV is invalid");
		AnimatedVector animatedVector = new AnimatedVector();
		string[] array = (xElement?.Attribute(XName.Get("viewBox"))?.Value ?? "0 0 0 0").Split(' ');
		animatedVector.ViewBox = new SKRect(float.Parse(array[0], CultureInfo.InvariantCulture), float.Parse(array[1], CultureInfo.InvariantCulture), float.Parse(array[2], CultureInfo.InvariantCulture), float.Parse(array[3], CultureInfo.InvariantCulture));
		int.TryParse(xElement?.Attribute(XName.Get("extend-bounds"))?.Value, CultureInfo.InvariantCulture, out var result);
		animatedVector.ExtendBounds = result;
		int num = 0;
		switch (xElement?.Attribute(XName.Get("stroke-linecap"))?.Value)
		{
		case "round":
			animatedVector.LineCap = SKStrokeCap.Round;
			break;
		case "butt":
			animatedVector.LineCap = SKStrokeCap.Butt;
			break;
		case "square":
			animatedVector.LineCap = SKStrokeCap.Square;
			break;
		default:
			animatedVector.LineCap = SKStrokeCap.Round;
			break;
		}
		switch (xElement?.Attribute(XName.Get("stroke-linejoin"))?.Value)
		{
		case "round":
			animatedVector.LineJoin = SKStrokeJoin.Round;
			break;
		case "bevel":
			animatedVector.LineJoin = SKStrokeJoin.Bevel;
			break;
		case "miter":
			animatedVector.LineJoin = SKStrokeJoin.Miter;
			break;
		default:
			animatedVector.LineJoin = SKStrokeJoin.Round;
			break;
		}
		float.TryParse(xElement?.Attribute(XName.Get("stroke-width"))?.Value ?? "0", CultureInfo.InvariantCulture, out var result2);
		string svgColor = xElement?.Attribute("stroke")?.Value ?? "#FFFFFF00";
		string svgColor2 = xElement?.Attribute("fill")?.Value ?? "#FFFFFF00";
		SKColor.TryParse(ConvertSvgHexToSkia(svgColor), out var color);
		SKColor.TryParse(ConvertSvgHexToSkia(svgColor2), out var color2);
		foreach (XElement item2 in xElement.Elements())
		{
			SKPath sKPath = null;
			switch (item2.Name.LocalName)
			{
			case "path":
				sKPath = ParsePath(ParsePathData(item2.Attribute("d")?.Value ?? ""));
				break;
			case "rect":
			{
				float.TryParse(item2.Attribute("x")?.Value ?? "0", CultureInfo.InvariantCulture, out var result10);
				float.TryParse(item2.Attribute("y")?.Value ?? "0", CultureInfo.InvariantCulture, out var result11);
				float.TryParse(item2.Attribute("width")?.Value ?? "0", CultureInfo.InvariantCulture, out var result12);
				float.TryParse(item2.Attribute("height")?.Value ?? "0", CultureInfo.InvariantCulture, out var result13);
				float.TryParse(item2.Attribute("rx")?.Value ?? "0", CultureInfo.InvariantCulture, out var result14);
				float.TryParse(item2.Attribute("ry")?.Value ?? result14.ToString(), CultureInfo.InvariantCulture, out var result15);
				sKPath = new SKPath();
				sKPath.AddRoundRect(SKRect.Create(result10, result11, result12, result13), result14, result15);
				break;
			}
			case "circle":
			{
				float.TryParse(item2.Attribute("cx")?.Value ?? "0", CultureInfo.InvariantCulture, out var result7);
				float.TryParse(item2.Attribute("cy")?.Value ?? "0", CultureInfo.InvariantCulture, out var result8);
				float.TryParse(item2.Attribute("r")?.Value ?? "0", CultureInfo.InvariantCulture, out var result9);
				if (result9 == 0f)
				{
					float.TryParse(item2.Attribute("rx")?.Value ?? "0", CultureInfo.InvariantCulture, out result9);
				}
				sKPath = new SKPath();
				sKPath.AddCircle(result7, result8, result9);
				break;
			}
			case "ellipse":
			{
				float.TryParse(item2.Attribute("cx")?.Value ?? "0", CultureInfo.InvariantCulture, out var result3);
				float.TryParse(item2.Attribute("cy")?.Value ?? "0", CultureInfo.InvariantCulture, out var result4);
				float.TryParse(item2.Attribute("rx")?.Value ?? "0", CultureInfo.InvariantCulture, out var result5);
				float.TryParse(item2.Attribute("ry")?.Value ?? "0", CultureInfo.InvariantCulture, out var result6);
				sKPath = new SKPath();
				sKPath.AddOval(SKRect.Create(result3 - result5, result4 - result6, result5 * 2f, result6 * 2f));
				break;
			}
			}
			if (sKPath != null)
			{
				animatedVector.Paths.Add(new AVPath
				{
					Fill = color2,
					Stroke = color,
					SKPath = sKPath,
					StrokeWidth = result2
				});
				num++;
			}
		}
		if (xElement2 != null && xElement2.Elements("animation") != null)
		{
			foreach (XElement item3 in xElement2.Elements("animation"))
			{
				Func<Func<float, float>> func = null;
				string[] array2 = (item3.Attribute("affected-paths")?.Value ?? "").Split(' ');
				int[] array3 = new int[array2.Length];
				if ((item3.Attribute("affected-paths")?.Value ?? "").Contains("*"))
				{
					array3 = new int[num];
					for (int i = 0; i < array3.Length; i++)
					{
						array3[i] = i;
					}
				}
				else
				{
					for (int j = 0; j < array2.Length; j++)
					{
						array3[j] = int.Parse(array2[j], CultureInfo.InvariantCulture);
					}
				}
				float.TryParse(item3.Attribute(XName.Get("duration"))?.Value ?? "", CultureInfo.InvariantCulture, out var duration);
				float.TryParse(item3.Attribute("extend-duration")?.Value ?? "", CultureInfo.InvariantCulture, out var result16);
				bool useObjectAnchor = (item3.Attribute("use-object-anchor")?.Value ?? "") == "true";
				bool useObjectSizeTranslation = (item3.Attribute("use-object-size-translation")?.Value ?? "") == "true";
				bool dontResetValues = (item3.Attribute("dont-reset")?.Value ?? "") == "true";
				bool perKeyframeEase = (item3.Attribute("per-keyframe-ease")?.Value ?? "") == "true";
				string text = item3.Attribute("easing")?.Value ?? "";
				if (text.StartsWith("cubic-bezier(") && text.EndsWith(")"))
				{
					text = text.Replace("cubic-bezier(", "");
					text = text.Replace(")", "");
					string[] easingValues = text.Split(',');
					try
					{
						func = () => BezierEasing.CreateEasing(float.Parse(easingValues[0], CultureInfo.InvariantCulture), float.Parse(easingValues[1], CultureInfo.InvariantCulture), float.Parse(easingValues[2], CultureInfo.InvariantCulture), float.Parse(easingValues[3], CultureInfo.InvariantCulture));
					}
					catch (Exception)
					{
						func = null;
						FLogger.Error("Unexpected values while trying to parse FAV animation easing");
					}
				}
				else if (text.StartsWith("spring(") && text.EndsWith(")"))
				{
					text = text.Replace("spring(", "");
					text = text.Replace(")", "");
					string[] easingValues2 = text.Split(',');
					try
					{
						func = delegate
						{
							Spring spring = new Spring(float.Parse(easingValues2[0], CultureInfo.InvariantCulture) / duration, float.Parse(easingValues2[1], CultureInfo.InvariantCulture));
							return (float x) => spring.Update(FContext.DeltaTime, Vector2.One * x).x;
						};
					}
					catch (Exception)
					{
						func = null;
						FLogger.Error("Unexpected values while trying to parse FAV animation easing");
					}
				}
				else if (text.StartsWith("snap-spring(") && text.EndsWith(")"))
				{
					text = text.Replace("snap-spring(", "");
					text = text.Replace(")", "");
					string[] easingValues3 = text.Split(',');
					try
					{
						func = delegate
						{
							Spring spring = new Spring(float.Parse(easingValues3[0], CultureInfo.InvariantCulture) / duration, float.Parse(easingValues3[1], CultureInfo.InvariantCulture));
							return (float x) => spring.Update(FContext.DeltaTime, Vector2.One).x;
						};
					}
					catch (Exception)
					{
						func = null;
						FLogger.Error("Unexpected values while trying to parse FAV animation easing");
					}
				}
				else if (text.Equals("linear"))
				{
					func = () => (float x) => x;
				}
				else if (text.Equals("ease"))
				{
					func = () => BezierEasing.Ease;
				}
				else if (text.Equals("ease-in"))
				{
					func = () => BezierEasing.EaseIn;
				}
				else if (text.Equals("ease-out"))
				{
					func = () => BezierEasing.EaseOut;
				}
				else if (text.Equals("ease-in-out"))
				{
					func = () => BezierEasing.EaseInOut;
				}
				else if (text.Equals("snap"))
				{
					func = () => (float x) => (x >= 0.5f) ? 1 : 0;
				}
				string item = item3.Attribute("id")?.Value ?? throw new Exception("Every animation must specify an ID");
				List<AVKeyframe> list = new List<AVKeyframe>();
				foreach (XElement item4 in item3.Elements("keyframe"))
				{
					if (item4 == null)
					{
						continue;
					}
					float.TryParse(item4.Attribute("time")?.Value ?? "0", CultureInfo.InvariantCulture, out var result17);
					List<(string, object)> list2 = new List<(string, object)>();
					string[] array4 = new string[10] { "translate-x", "translate-y", "scale-x", "scale-y", "anchor-x", "anchor-y", "rotate", "opacity", "blur-radius", "stroke-trace" };
					foreach (string text2 in array4)
					{
						XElement xElement3 = item4.Element(text2);
						if (xElement3 != null && float.TryParse(xElement3.Attribute("value")?.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result18))
						{
							list2.Add((text2, result18));
						}
					}
					list.Add(new AVKeyframe
					{
						time = result17,
						attributes = list2
					});
				}
				if (!list.Any((AVKeyframe x) => x.time == 1f))
				{
					list.Add(new AVKeyframe
					{
						time = 1f
					});
				}
				if (!list.Any((AVKeyframe x) => x.time == 0f))
				{
					list.Add(new AVKeyframe
					{
						time = 0f
					});
				}
				animatedVector.Animations.Add((item, new AVAnimation
				{
					AffectedPathIDs = array3,
					Duration = duration,
					Keyframes = list,
					UseObjectAnchor = useObjectAnchor,
					UseObjectSizeTranslation = useObjectSizeTranslation,
					DontResetValues = dontResetValues,
					ExtendDuration = result16,
					PerKeyframeEase = perKeyframeEase,
					CreateEasing = (func ?? ((Func<Func<float, float>>)(() => (float x) => x)))
				}));
			}
		}
		return animatedVector;
	}

	private static string ConvertSvgHexToSkia(string svgColor)
	{
		if (string.IsNullOrWhiteSpace(svgColor) || !svgColor.StartsWith("#"))
		{
			return svgColor;
		}
		svgColor = svgColor.Trim();
		if (svgColor.Length == 7)
		{
			return "#FF" + svgColor.Substring(1);
		}
		if (svgColor.Length == 9)
		{
			string text = svgColor.Substring(1, 6);
			string text2 = svgColor.Substring(7, 2);
			return "#" + text2 + text;
		}
		return svgColor;
	}

	private static SKPath ParsePath(List<(char command, List<float> arguments)> data)
	{
		SKPath sKPath = new SKPath();
		SKPoint sKPoint = new SKPoint(0f, 0f);
		new SKPoint(0f, 0f);
		SKPoint sKPoint2 = new SKPoint(0f, 0f);
		SKPoint sKPoint3 = new SKPoint(0f, 0f);
		bool flag = false;
		bool flag2 = false;
		foreach (var datum in data)
		{
			var (c, list) = datum;
			if (char.ToUpper(datum.command) == 'Z')
			{
				sKPath.Close();
				continue;
			}
			int num = 0;
			while (num < list.Count)
			{
				switch (c)
				{
				case 'M':
					sKPoint = new SKPoint(list[num], list[num + 1]);
					sKPath.MoveTo(sKPoint);
					num += 2;
					c = 'L';
					break;
				case 'm':
					sKPoint = new SKPoint(sKPoint.X + list[num], sKPoint.Y + list[num + 1]);
					sKPath.MoveTo(sKPoint);
					num += 2;
					c = 'l';
					break;
				case 'L':
					sKPoint = new SKPoint(list[num], list[num + 1]);
					sKPath.LineTo(sKPoint);
					num += 2;
					break;
				case 'l':
					sKPoint = new SKPoint(sKPoint.X + list[num], sKPoint.Y + list[num + 1]);
					sKPath.LineTo(sKPoint);
					num += 2;
					break;
				case 'H':
					sKPoint = new SKPoint(list[num], sKPoint.Y);
					sKPath.LineTo(sKPoint);
					num++;
					break;
				case 'h':
					sKPoint = new SKPoint(sKPoint.X + list[num], sKPoint.Y);
					sKPath.LineTo(sKPoint);
					num++;
					break;
				case 'V':
					sKPoint = new SKPoint(sKPoint.X, list[num]);
					sKPath.LineTo(sKPoint);
					num++;
					break;
				case 'v':
					sKPoint = new SKPoint(sKPoint.X, sKPoint.Y + list[num]);
					sKPath.LineTo(sKPoint);
					num++;
					break;
				case 'C':
				{
					SKPoint point = new SKPoint(list[num], list[num + 1]);
					SKPoint sKPoint11 = new SKPoint(list[num + 2], list[num + 3]);
					SKPoint sKPoint12 = new SKPoint(list[num + 4], list[num + 5]);
					sKPath.CubicTo(point, sKPoint11, sKPoint12);
					sKPoint2 = sKPoint11;
					flag = true;
					sKPoint = sKPoint12;
					flag2 = false;
					num += 6;
					break;
				}
				case 'c':
				{
					SKPoint point4 = new SKPoint(sKPoint.X + list[num], sKPoint.Y + list[num + 1]);
					SKPoint sKPoint20 = new SKPoint(sKPoint.X + list[num + 2], sKPoint.Y + list[num + 3]);
					SKPoint sKPoint21 = new SKPoint(sKPoint.X + list[num + 4], sKPoint.Y + list[num + 5]);
					sKPath.CubicTo(point4, sKPoint20, sKPoint21);
					sKPoint2 = sKPoint20;
					flag = true;
					sKPoint = sKPoint21;
					flag2 = false;
					num += 6;
					break;
				}
				case 'S':
				{
					SKPoint point3 = (flag ? new SKPoint(2f * sKPoint.X - sKPoint2.X, 2f * sKPoint.Y - sKPoint2.Y) : sKPoint);
					SKPoint sKPoint16 = new SKPoint(list[num], list[num + 1]);
					SKPoint sKPoint17 = new SKPoint(list[num + 2], list[num + 3]);
					sKPath.CubicTo(point3, sKPoint16, sKPoint17);
					sKPoint2 = sKPoint16;
					flag = true;
					sKPoint = sKPoint17;
					flag2 = false;
					num += 4;
					break;
				}
				case 's':
				{
					SKPoint point2 = (flag ? new SKPoint(2f * sKPoint.X - sKPoint2.X, 2f * sKPoint.Y - sKPoint2.Y) : sKPoint);
					SKPoint sKPoint14 = new SKPoint(sKPoint.X + list[num], sKPoint.Y + list[num + 1]);
					SKPoint sKPoint15 = new SKPoint(sKPoint.X + list[num + 2], sKPoint.Y + list[num + 3]);
					sKPath.CubicTo(point2, sKPoint14, sKPoint15);
					sKPoint2 = sKPoint14;
					flag = true;
					sKPoint = sKPoint15;
					flag2 = false;
					num += 4;
					break;
				}
				case 'Q':
				{
					SKPoint sKPoint9 = new SKPoint(list[num], list[num + 1]);
					SKPoint sKPoint10 = new SKPoint(list[num + 2], list[num + 3]);
					sKPath.QuadTo(sKPoint9, sKPoint10);
					sKPoint3 = sKPoint9;
					flag2 = true;
					flag = false;
					sKPoint = sKPoint10;
					num += 4;
					break;
				}
				case 'q':
				{
					SKPoint sKPoint7 = new SKPoint(sKPoint.X + list[num], sKPoint.Y + list[num + 1]);
					SKPoint sKPoint8 = new SKPoint(sKPoint.X + list[num + 2], sKPoint.Y + list[num + 3]);
					sKPath.QuadTo(sKPoint7, sKPoint8);
					sKPoint3 = sKPoint7;
					flag2 = true;
					flag = false;
					sKPoint = sKPoint8;
					num += 4;
					break;
				}
				case 'T':
				{
					SKPoint sKPoint5 = (flag2 ? new SKPoint(2f * sKPoint.X - sKPoint3.X, 2f * sKPoint.Y - sKPoint3.Y) : sKPoint);
					SKPoint sKPoint6 = new SKPoint(list[num], list[num + 1]);
					sKPath.QuadTo(sKPoint5, sKPoint6);
					sKPoint3 = sKPoint5;
					flag2 = true;
					flag = false;
					sKPoint = sKPoint6;
					num += 2;
					break;
				}
				case 't':
				{
					SKPoint sKPoint18 = (flag2 ? new SKPoint(2f * sKPoint.X - sKPoint3.X, 2f * sKPoint.Y - sKPoint3.Y) : sKPoint);
					SKPoint sKPoint19 = new SKPoint(sKPoint.X + list[num], sKPoint.Y + list[num + 1]);
					sKPath.QuadTo(sKPoint18, sKPoint19);
					sKPoint3 = sKPoint18;
					flag2 = true;
					flag = false;
					sKPoint = sKPoint19;
					num += 2;
					break;
				}
				case 'A':
				{
					float x2 = list[num];
					float y2 = list[num + 1];
					float xAxisRotate2 = list[num + 2];
					bool flag5 = list[num + 3] != 0f;
					bool flag6 = list[num + 4] != 0f;
					SKPoint sKPoint13 = new SKPoint(list[num + 5], list[num + 6]);
					sKPath.ArcTo(new SKPoint(x2, y2), xAxisRotate2, flag5 ? SKPathArcSize.Large : SKPathArcSize.Small, (!flag6) ? SKPathDirection.CounterClockwise : SKPathDirection.Clockwise, sKPoint13);
					sKPoint = sKPoint13;
					flag = false;
					flag2 = false;
					num += 7;
					break;
				}
				case 'a':
				{
					float x = list[num];
					float y = list[num + 1];
					float xAxisRotate = list[num + 2];
					bool flag3 = list[num + 3] != 0f;
					bool flag4 = list[num + 4] != 0f;
					SKPoint sKPoint4 = new SKPoint(sKPoint.X + list[num + 5], sKPoint.Y + list[num + 6]);
					sKPath.ArcTo(new SKPoint(x, y), xAxisRotate, flag3 ? SKPathArcSize.Large : SKPathArcSize.Small, (!flag4) ? SKPathDirection.CounterClockwise : SKPathDirection.Clockwise, sKPoint4);
					sKPoint = sKPoint4;
					flag = false;
					flag2 = false;
					num += 7;
					break;
				}
				default:
					throw new NotSupportedException($"SVG command '{c}' is not supported.");
				}
			}
		}
		return sKPath;
	}

	public static List<(char command, List<float> arguments)> ParsePathData(string path)
	{
		List<(char, List<float>)> list = new List<(char, List<float>)>();
		MatchCollection matchCollection = new Regex("([MmZzLlHhVvCcSsQqTtAa])|(-?\\d*\\.?\\d+(?:e[-+]?\\d+)?)", RegexOptions.IgnoreCase).Matches(path);
		char c = '\0';
		List<float> list2 = new List<float>();
		foreach (Match item in matchCollection)
		{
			if (item.Groups[1].Success)
			{
				if (c != 0)
				{
					list.Add((c, new List<float>(list2)));
				}
				c = item.Groups[1].Value[0];
				list2.Clear();
			}
			else if (item.Groups[2].Success)
			{
				list2.Add(float.Parse(item.Groups[2].Value, CultureInfo.InvariantCulture));
			}
		}
		if (c != 0)
		{
			list.Add((c, list2));
		}
		return list;
	}
}
