using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Mutagen.Bethesda.FormKeys.SkyrimSE;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Immutable;

namespace DisplaySpellTomeLevelPatcher
{
    public class Program
    {
        private static Lazy<Settings> _settings = null!;
        public static Task<int> Main(string[] args)
        {
            return SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetAutogeneratedSettings(nickname: "Settings", path: "settings.json", out _settings)
                .SetTypicalOpen(GameRelease.SkyrimSE, "DisplaySpellTomeLevelPatcher.esp")
                .Run(args);
        }

        public static ModKey Vokrii = ModKey.FromNameAndExtension("Vokrii - Minimalistic Perks of Skyrim.esp");

        public static readonly HashSet<string> skillLevels = new HashSet<string>() {
            "Novice",

            "Apprenti",
            "Adepte",
            "Expert",
            "Maître"
        };

        public static readonly HashSet<string> magicSchools = new HashSet<string>()
        {
            "Guérison",
            "Destruction",
            "Conjuration",
            "Illusion",
            "Altération"
        };

        public const string levelFormatVariable = "<level>";
        public const string spellFormatVariable = "<spell>";
        public const string pluginFormatVariable = "<plugin>";
        public const string schoolFormatVariable = "<school>";

        public static Dictionary<string, string> spellLevelDictionary = new Dictionary<string, string>();

        public static string GetSpellNameFromSpellTome(string spellTomeName)
        {
            try
            {
                if (spellTomeName.Contains(" - ")) {
                    return spellTomeName.Split(" - ")[1];
                }
                return spellTomeName.Split(": ")[1];
            }
            catch (IndexOutOfRangeException)
            {
                return "";
            }
        }

        public static string GetSpellNameFromScroll(string scrollName)
        {
            string[] splitScrollName = scrollName.Split(' ');
            string scrollSpellName = string.Join(' ', splitScrollName.Skip(2).ToArray());
            return scrollSpellName;
        }

        public static bool NamedFieldsContain<TMajor>(TMajor named, string str)
            where TMajor : INamedGetter, IMajorRecordCommonGetter
        {
            if (named.EditorID?.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (named.Name?.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0) return true;

            return false;
        }

        public static bool DescriptionContain(IPerkGetter perkGetter, string str)
        {
            return perkGetter.Description?.String?.IndexOf(str, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            foreach (var bookContext in state.LoadOrder.PriorityOrder.Book().WinningContextOverrides())
            {
                IBookGetter book = bookContext.Record;

                if (book.Name?.String == null) continue;
                if (!book.Keywords?.Contains(Skyrim.Keyword.VendorItemSpellTome) ?? true) continue;
                if (book.Teaches is not IBookSpellGetter teachedSpell) continue;
                if (!teachedSpell.Spell.TryResolve(state.LinkCache, out var spell)) continue;
                if (!state.LinkCache.TryResolveContext(spell.HalfCostPerk.FormKey, spell.HalfCostPerk.Type, out var halfCostPerkContext)) continue;
                var halfCostPerk = (IPerkGetter)halfCostPerkContext.Record;
                if (halfCostPerk == null) continue;

                
                string i18nBookName = "";
                
                if (!book.Name.TryLookup(Language.French, out i18nBookName)) {
                    //Console.WriteLine($"{book.FormKey}: Pas de traduction pour: {book.Name.String}");
                    i18nBookName = book.Name.String;
                }
                
                //Console.WriteLine($"{book.FormKey}: Traduction: {i18nBookName}");

                string spellName = GetSpellNameFromSpellTome(i18nBookName);
                if (spellName == "")
                {
                    Console.WriteLine($"{book.FormKey}: Could not get spell name from: {i18nBookName}");

                    continue;
                }

                string bookName = _settings.Value.Format;
                bool changed = false;
                if (bookName.Contains(levelFormatVariable))
                {
                    foreach (string skillLevel in skillLevels)
                    {

                        //string i18nSkillLevel = Encoding.GetEncoding("ISO-8859-1").GetString(Encoding.UTF8.GetBytes(skillLevel));
                        string i18nSkillLevel = skillLevel;
                        if (halfCostPerkContext.ModKey == Vokrii && halfCostPerk.Description != null)
                        {
                            if (!DescriptionContain(halfCostPerk, i18nSkillLevel)) continue;
                        }
                        else if (!NamedFieldsContain(halfCostPerk, i18nSkillLevel)) continue;


                        bookName = bookName.Replace(levelFormatVariable, i18nSkillLevel);

                        changed = true;
                        break;
                    }
                }
                if (halfCostPerkContext.ModKey == Vokrii && bookName.Contains(levelFormatVariable))
                {
                    bookName.Replace(levelFormatVariable, "Novice");
                }
                if (bookName.Contains(pluginFormatVariable))
                {
                    bookName = bookName.Replace(pluginFormatVariable, book.FormKey.ModKey.Name.ToString());
                    changed = true;
                }
                if (bookName.Contains(schoolFormatVariable))
                {
                    foreach (string spellSchool in magicSchools)
                    {
                        //string i18nSpellSchool = Encoding.GetEncoding("ISO-8859-1").GetString(Encoding.UTF8.GetBytes(spellSchool));
                        string i18nSpellSchool = spellSchool;
                        if (NamedFieldsContain(halfCostPerk, i18nSpellSchool) || DescriptionContain(halfCostPerk, i18nSpellSchool))
                        {
                            bookName = bookName.Replace(schoolFormatVariable, i18nSpellSchool);
                            changed = true;
                            break;
                        }
                    }
                }
                if (bookName.Contains(spellFormatVariable))
                {

                    bookName = bookName.Replace(spellFormatVariable, GetSpellNameFromSpellTome(i18nBookName));
                    changed = true;
                }
                if (changed && i18nBookName != bookName)

                {
                    string i18nBookDescription = null;
                    string i18nBookText = null;
                    book.Description?.TryLookup(Language.French, out i18nBookDescription);
                    book.BookText?.TryLookup(Language.French, out i18nBookText);

                    Book bookToAdd = book.DeepCopy();
                    bookToAdd.Name = bookName;
                    bookToAdd.Description = i18nBookDescription ?? book.Description.String;
                    bookToAdd.BookText = i18nBookText ?? book.BookText.String;

                    Console.WriteLine($"{book.FormKey}: {bookName} : {bookToAdd.BookText.String}");
                    state.PatchMod.Books.Set(bookToAdd);
                }
            }
        }
    }
}
