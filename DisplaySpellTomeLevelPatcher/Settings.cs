using Mutagen.Bethesda.Synthesis.Settings;
using System.Collections.Generic;

namespace DisplaySpellTomeLevelPatcher
{
    public class Settings
    {
        [SynthesisTooltip(@"Choisissez votre propre format ici ! Les variables disponibles sont : <level> (ex. Adepte), <spell> (ex. Clairvoyance), <plugin> (ex. Skyrim), <mod> (nom du mod au lieu du nom du plugin, ex. Forgotten Magic Redone), <school> (ex. Alteration). Le format par défaut est : Livre de sort (<level>) - <spell>")]
        public string Format { get; set; } = "Livre de sort (<level>) - <spell>";

        [SynthesisTooltip(@"Spécifiez votre propre format pour les noms de mod (<mod>) ici ! Lorsqu'un nom de plugin n'est pas trouvé ici, le patcher essaiera de convertir automatiquement le nom du plugin en nom de mod - les résultats peuvent varier.")]
        public Dictionary<string, string> PluginModNamePairs { get; set; } = new()
        {
            { "Skyrim.esm", "Skyrim" },
            { "Dawnguard.esm", "Dawnguard" },
            { "Dragonborn.esm", "Dragonborn" },
            { "HearthFires.esm", "HearthFires" },
            { "Apocalypse - Magic of Skyrim.esp", "Apocalypse" },
            { "Arcanum.esp", "Arcanum" },
            { "Triumvirate - Mage Archetypes.esp", "Triumvirate" },
            { "ForgottenMagic_Redone.esp", "Forgotten Magic Redone" },
            { "Phenderix Magic Evolved.esp", "Phenderix Magic Evolved" },
            { "ShadowSpellPackage.esp", "Shadow Spell Package" },
            { "PathOfTheAntiMage.esp", "Path of the Anti-Mage" }
        };

        [SynthesisTooltip(@"Ce sont les noms de niveau qui seront utilisés pour <level>. Vous pouvez éventuellement les raccourcir ou les remplacer par un autre nom ici. Par défault : Novice, Apprenti, Adepte, Expert, Maître")]
        public List<string> LevelNames { get; set; } = new() {
            "Novice",
            "Apprenti",
            "Adepte",
            "Expert",
            "Maître"
        };
    }
}
