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
using System.Text.RegularExpressions;
using System.Collections.Immutable;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Strings;

namespace DisplaySpellTomeLevelPatcher
{
    public class Program
{
    public static void Main(string[] args)
    {
        using var env = GameEnvironment.Typical.Skyrim(SkyrimRelease.SkyrimSE);
        var skyrimModKey = ModKey.FromFileName("Skyrim.esm");
        var skyrim = env.LoadOrder.GetIfEnabledAndExists(skyrimModKey);

        // Just inspect/process one specific book, for testing
        var Oakflesh = skyrim.Books.First(x => x.EditorID == "SpellTomeOakflesh");
        TestOnBook(skyrimModKey, Oakflesh);
    }

    private static void TestOnBook(ModKey modKey, IBookGetter book)
    {
        if (book.Name == null) return;
        var name = book.Name;
        
        Console.WriteLine($"Name for {book} from {modKey} is {name}");
        Console.WriteLine($"Had {book.Name.NumLanguages} registered languages");
            
        // Bounce off the strings files to get the french version
        if (book.Name.TryLookup(Language.French, out var fre))
        {
            Console.WriteLine($"Had name in French: {fre}");
        }

        // Make a new mod to write out
        var outgoing = new SkyrimMod("Oakflesh.esp", SkyrimRelease.SkyrimSE);

        // Add book as override
        var bookW = outgoing.Books.GetOrAddAsOverride(book);

        // Set the name to the french version + "Test"
        bookW.Name = $"{fre}Test";

        outgoing.WriteToBinary(outgoing.ModKey.FileName.ToString());
    }
}

    
}
