using Elements.Assets;
using Elements.Core;

using FrooxEngine;

using HarmonyLib;

using ResoniteModLoader;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text.Unicode;

using static OfficialAssets.Graphics;

namespace CJKLinewrap;
//More info on creating mods can be found https://github.com/resonite-modding-group/ResoniteModLoader/wiki/Creating-Mods
public class CJKLinewrap : ResoniteMod {
	internal const string VERSION_CONSTANT = "0.0.1"; //Changing the version here updates it in all locations needed
	public override string Name => "CJKLinewrap";
	public override string Author => "Meow Wei 魏喵";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/resonite-modding-group/CJKLinewrap/";

	private static readonly UnicodeRange[] testRanges = {
		UnicodeRanges.HangulJamo,
		UnicodeRanges.Hiragana,
		UnicodeRanges.Katakana,
		UnicodeRanges.CjkRadicalsSupplement,
		UnicodeRanges.CjkSymbolsandPunctuation,
		UnicodeRanges.HalfwidthandFullwidthForms,
		UnicodeRanges.EnclosedCjkLettersandMonths,
		UnicodeRanges.CjkCompatibility,
		UnicodeRanges.CjkUnifiedIdeographs,
		UnicodeRanges.CjkUnifiedIdeographsExtensionA,
		UnicodeRanges.HangulSyllables,
		UnicodeRanges.CjkCompatibilityForms
	};

	public override void OnEngineInit() {
		Harmony harmony = new Harmony("net.m-wei.CJKLinewrap");
		harmony.PatchAll();
	}

	private static readonly string noBeginsWith = "!\"%'),.:;?]}°·»‐–—†‡•›‼⁇⁈⁉∶、。〃〉》」』】〕〗〙〜〞〟゠・︰︱︲︳︶︸︺︼︾﹀﹂﹐﹑﹒﹔﹕﹖﹗﹘﹚﹜！＂％＇），．：；？］｜｝～｠､々〻ーヽヾ¢℃ぁァぃィぅゥぇェぉォゕヵㇰゖヶㇱ〆ㇲっッㇳㇴㇵㇶㇷㇸㇹㇺゃャゅュょョㇻㇼㇽㇾㇿゎヮ";
	private static readonly string noEndsWith = "\"#'([\\{«·‵〈《「『【〔〖〘〝︴︵︷︹︻︽︿﹁﹃﹏﹙﹛（．［｛｟々$＄£￡¥￥￦〇";

	[HarmonyPatch(typeof(StringRenderTree), "GenerateLines")]
	class StringRenderTree_GenerateLines_Patch {
		static bool Prefix(
			StringRenderTree __instance,
			ref List<StringLine> ____lines,
			float ____invTabLength,
			float ____tabLength,
			RenderGlyph[] ____glyphLayout,
			GlyphPositioner glyphPositionOverride
		) {
			GenerateLines(__instance, ref ____lines, ____invTabLength, ____tabLength, ____glyphLayout, glyphPositionOverride);
			return false;
		}
	}

	private static bool AllowsLineBreak(UnicodeRange[] ranges, char ch1, char ch2) {
		if (noBeginsWith.IndexOf(ch2) >= 0) {
			return false;
		}
		if (noEndsWith.IndexOf(ch1) >= 0) {
			return false;
		}
		if (ch2 == '\n' || char.IsWhiteSpace(ch2) || ch2 == '\u200b') {
			return true;
		}
		if (ch2 < 256) {
			return ch1 >= 256; // allow line breaks if one of them is not from ascii...?
		}
		foreach (var range in ranges) {
			if (ch2 >= range.FirstCodePoint && ch2 < range.FirstCodePoint + range.Length) {
				return true;
			}
		}
		return false;
	}

	private static void GenerateLines(
			StringRenderTree that,
			ref List<StringLine> _lines,
			float _invTabLength,
			float _tabLength,
			RenderGlyph[] _glyphLayout,
			GlyphPositioner glyphPositionOverride
		) {
		if (glyphPositionOverride == null) {
			if (_lines == null) {
				_lines = new List<StringLine>();
			} else {
				_lines.Clear();
			}
		}
		float2 offset = float2.Zero;
		bool dontResetOffset = false;
		int currentLineIndex = 0;
		int currentLineStart = 0;
		int lineCount = 0;
		foreach (StringRenderGlyphSegmentNode segment in that) {
			int blockLineBreakFrom;
			if (segment.DisableLineBreaking()) {
				blockLineBreakFrom = segment.GlyphSegmentOffset;
			} else {
				blockLineBreakFrom = -1;
			}
			int i = 0;
			char prevCh = '\0';
			char ch = '\0';
			while (i < segment.GlyphSegmentLength) {
				int currentGlyphIndex = segment.GlyphSegmentOffset + i;
				ref RenderGlyph glyph = ref segment.GetRenderGlyph(i);
				while (currentLineIndex >= lineCount) {
					if (glyphPositionOverride == null) {
						_lines.Add(new StringLine());
					}
					lineCount++;
					if (!dontResetOffset) {
						offset = new float2(-glyph.rect.x, 0f);
					}
					dontResetOffset = false;
				}
				if (glyphPositionOverride == null) {
					glyph.line = currentLineIndex;
					glyph.Translate(in offset);
				} else {
					glyphPositionOverride(ref glyph, in offset, currentLineIndex);
				}
				bool canBreakBefore = false;
				if (segment.HasCharacters) {
					prevCh = ch;
					ch = segment.GetCharacter(glyph.stringIndex);
					if (ch != '\t') {
						if (ch == '\n') {
							currentLineIndex++;
							currentLineStart = currentGlyphIndex + 1;
							goto IL_0283;
						}
						if (char.IsControl(ch)) {
							goto IL_0283;
						}
					} else {
						float xmax = glyph.rect.xmax;
						float tabGlyphOffset = (float)MathX.CeilToInt(xmax * _invTabLength) * _tabLength - xmax;
						ref RenderGlyph ptr = ref glyph;
						ptr.Translate(new float2(tabGlyphOffset, 0f));
						offset += new float2(tabGlyphOffset, 0f);
					}
					canBreakBefore = AllowsLineBreak(testRanges, prevCh, ch);
					goto IL_0191;
				}
				goto IL_0191;
			IL_0283:
				i++;
				continue;
			IL_0191:
				if (glyphPositionOverride == null && that.Bounded && ExceedsHorizontalBounds(that, ref glyph)) {
					int breakCharacterIndex = currentGlyphIndex;
					int breakStartSearch = currentGlyphIndex - 1;
					if (blockLineBreakFrom >= 0) {
						breakStartSearch = blockLineBreakFrom - 1;
					}
					for (int j = breakStartSearch; j >= currentLineStart; j--) {
						char ch1 = j-1>=currentLineStart?that.String[_glyphLayout[j-1].stringIndex]:'\0';
						char ch2 = that.String[_glyphLayout[j].stringIndex];
						if (AllowsLineBreak(testRanges, ch1, ch2)) {
							breakCharacterIndex = j;
							break;
						}
					}
					if (breakCharacterIndex != currentGlyphIndex || !canBreakBefore) {
						float wrapOffset = -_glyphLayout[breakCharacterIndex].rect.x;
						if (glyphPositionOverride == null) {
							for (int k = breakCharacterIndex; k <= currentGlyphIndex; k++) {
								ref RenderGlyph renderGlyph = ref that.GetRenderGlyph(k);
								renderGlyph.line++;
								renderGlyph.Translate(wrapOffset, 0f);
							}
						}
						offset += new float2(wrapOffset, 0f);
						dontResetOffset = true;
					} else {
						breakCharacterIndex++;
					}
					currentLineIndex++;
					currentLineStart = breakCharacterIndex;
					goto IL_0283;
				}
				goto IL_0283;
			}
			offset += segment.PenPosition;
		}
		if (glyphPositionOverride == null && currentLineIndex == that.LineCount) {
			_lines.Add(new StringLine());
		}
	}
	private static bool ExceedsHorizontalBounds(StringRenderTree that, ref RenderGlyph glyph) {
		return glyph.rect.xmax - that.BoundsSize.x > that.ActualSize * 0.001f;
	}
}
