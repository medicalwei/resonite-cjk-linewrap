using Elements.Assets;

using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text.Unicode;

using static OfficialAssets.Graphics;

namespace CJKLinewrap;
//More info on creating mods can be found https://github.com/resonite-modding-group/ResoniteModLoader/wiki/Creating-Mods
public class CJKLinewrap : ResoniteMod {
	internal const string VERSION_CONSTANT = "0.0.1"; //Changing the version here updates it in all locations needed
	public override string Name => "CJKLinewrap";
	public override string Author => "ExampleAuthor";
	public override string Version => VERSION_CONSTANT;
	public override string Link => "https://github.com/resonite-modding-group/CJKLinewrap/";

	private static readonly UnicodeRange[] testRanges = {
		UnicodeRanges.HangulJamo,
		UnicodeRanges.CjkRadicalsSupplement,
		UnicodeRanges.CjkSymbolsandPunctuation,
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


	[HarmonyPatch(typeof(StringRenderTree), nameof(StringRenderTree.AllowsLineBreak))]
	class StringRenderTree_AllowsLineBreak_Patch {
		static void Postfix(ref bool __result, char ch) {
			if (ch <= 255) {
				return;
			}
			__result = UnicodeRangesContain(testRanges, ch);
		}
	}

	private static bool UnicodeRangesContain(UnicodeRange[] ranges, char ch) {
		foreach (var range in ranges) {
			if (ch >= range.FirstCodePoint && ch < range.FirstCodePoint+range.Length) {
				return true;
			}
		}
		return false;
	}
}
