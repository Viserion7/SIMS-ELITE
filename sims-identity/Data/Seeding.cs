namespace sims_identity.Data;

using BCrypt.Net;
using Microsoft.EntityFrameworkCore;


public class Seeder
{
    private ApplicationDbContext context { get; set; }
    public Seeder(ApplicationDbContext contextInp)
    {
        context = contextInp;

    }
    public async Task AddCategories()
    {

        try
        {
            await context.Database.EnsureCreatedAsync();

            if (!await context.User.AnyAsync())
            {
                context.User.Add(new User
                {
                    email = "admin@local",
                    password_hash = BCrypt.HashPassword("admin"),
                    is_deleted = false,
                    is_Admin = true,
                    is_ToNotify = true

                });

                await context.SaveChangesAsync();
            }

            List<Level> levels = new List<Level>
            {
                new Level{
                name = "Low"
                },
                new Level{
                name = "Medium"
                },
                new Level{
                name = "High"
                },
            };

            foreach (Level item in levels)
            {
                Level? bestehendesItem = context.Level
                .FirstOrDefault(inDbSchonExestierend => inDbSchonExestierend.name == item.name);
                if (bestehendesItem == null)
                {
                    context.Level.Add(item);
                }
                else
                {
                    bestehendesItem.name = item.name;
                }

            }

            await context.SaveChangesAsync();



            int lowLevel = context.Level.First(level => level.name == "Low").id;
            int mediumLevel = context.Level.First(level => level.name == "Medium").id;
            int highLevel = context.Level.First(level => level.name == "High").id;



            List<Category> categories = new List<Category>
                {
                    // STIX Domain Objects
                    new Category
                    {
                        name = "attack-pattern",
                        description = "STIX beschreibt, wie ein Angriff ausgeführt wird.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "campaign",
                        description = "STIX beschreibt eine zusammengehörige Angriffskampagne.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "course-of-action",
                        description = "STIX beschreibt eine empfohlene Gegenmaßnahme.",
                        LevelId = lowLevel
                    },
                    new Category
                    {
                        name = "grouping",
                        description = "STIX gruppiert zusammengehörige Objekte.",
                        LevelId = lowLevel
                    },
                    new Category
                    {
                        name = "identity",
                        description = "STIX beschreibt eine Person, Organisation oder ein System.",
                        LevelId = lowLevel
                    },
                    new Category
                    {
                        name = "incident",
                        description = "STIX beschreibt einen Sicherheitsvorfall.",
                        LevelId = highLevel
                    },
                    new Category
                    {
                        name = "indicator",
                        description = "STIX beschreibt ein Muster zur Erkennung möglicher Bedrohungen.",
                        LevelId = highLevel
                    },
                    new Category
                    {
                        name = "infrastructure",
                        description = "STIX beschreibt technische Infrastruktur einer Bedrohung.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "intrusion-set",
                        description = "STIX beschreibt eine Gruppe oder Sammlung von Angriffen.",
                        LevelId = highLevel
                    },
                    new Category
                    {
                        name = "location",
                        description = "STIX beschreibt einen geografischen Ort.",
                        LevelId = lowLevel
                    },
                    new Category
                    {
                        name = "malware",
                        description = "STIX beschreibt Schadsoftware.",
                        LevelId = highLevel
                    },
                    new Category
                    {
                        name = "malware-analysis",
                        description = "STIX beschreibt die Analyse von Schadsoftware.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "note",
                        description = "STIX speichert zusätzliche Notizen zu anderen Objekten.",
                        LevelId = lowLevel
                    },
                    new Category
                    {
                        name = "observed-data",
                        description = "STIX beschreibt beobachtete Daten aus einem Netzwerk oder System.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "opinion",
                        description = "STIX beschreibt eine Bewertung oder Einschätzung.",
                        LevelId = lowLevel
                    },
                    new Category
                    {
                        name = "report",
                        description = "STIX fasst Informationen zu einer Bedrohung zusammen.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "threat-actor",
                        description = "STIX beschreibt einen Angreifer oder eine Angreifergruppe.",
                        LevelId = highLevel
                    },
                    new Category
                    {
                        name = "tool",
                        description = "STIX beschreibt ein Werkzeug, das bei Angriffen verwendet wird.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "vulnerability",
                        description = "STIX beschreibt eine Schwachstelle in einem System.",
                        LevelId = highLevel
                    },

                    // STIX Cyber-observable Objects
                    new Category
                    {
                        name = "artifact",
                        description = "STIX beschreibt beliebige Binärdaten oder Dateien.",
                        LevelId = lowLevel
                    },
                    new Category
                    {
                        name = "autonomous-system",
                        description = "STIX beschreibt ein autonomes Netzwerk-System.",
                        LevelId = lowLevel
                    },
                    new Category
                    {
                        name = "directory",
                        description = "STIX beschreibt ein Verzeichnis im Dateisystem.",
                        LevelId = lowLevel
                    },
                    new Category
                    {
                        name = "domain-name",
                        description = "STIX beschreibt einen Domainnamen.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "email-addr",
                        description = "STIX beschreibt eine E-Mail-Adresse.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "email-message",
                        description = "STIX beschreibt eine E-Mail-Nachricht.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "file",
                        description = "STIX beschreibt eine Datei.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "ipv4-addr",
                        description = "STIX beschreibt eine IPv4-Adresse.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "ipv6-addr",
                        description = "STIX beschreibt eine IPv6-Adresse.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "mac-addr",
                        description = "STIX beschreibt eine MAC-Adresse.",
                        LevelId = lowLevel
                    },
                    new Category
                    {
                        name = "mutex",
                        description = "STIX beschreibt ein Mutex-Objekt.",
                        LevelId = lowLevel
                    },
                    new Category
                    {
                        name = "network-traffic",
                        description = "STIX beschreibt Netzwerkverkehr.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "process",
                        description = "STIX beschreibt einen laufenden Prozess.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "software",
                        description = "STIX beschreibt Software oder ein Betriebssystem.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "url",
                        description = "STIX beschreibt eine URL.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "user-account",
                        description = "STIX beschreibt ein Benutzerkonto.",
                        LevelId = highLevel
                    },
                    new Category
                    {
                        name = "windows-registry-key",
                        description = "STIX beschreibt einen Windows-Registrierungsschlüssel.",
                        LevelId = mediumLevel
                    },
                    new Category
                    {
                        name = "x509-certificate",
                        description = "STIX beschreibt ein X.509-Zertifikat.",
                        LevelId = mediumLevel
                    },

                    // STIX Relationship Objects
                    new Category
                    {
                        name = "relationship",
                        description = "STIX beschreibt eine Beziehung zwischen zwei Objekten.",
                        LevelId = lowLevel
                    },
                    new Category
                    {
                        name = "sighting",
                        description = "STIX beschreibt das Beobachten eines Objekts.",
                        LevelId = mediumLevel
                    },

                    // STIX Meta Objects
                    new Category
                    {
                        name = "language-content",
                        description = "STIX enthält Übersetzungen und sprachabhängige Inhalte.",
                        LevelId = lowLevel
                    },
                    new Category
                    {
                        name = "marking-definition",
                        description = "STIX beschreibt Kennzeichnungen und Zugriffsbeschränkungen.",
                        LevelId = lowLevel
                    },
                    new Category
                    {
                        name = "extension-definition",
                        description = "STIX beschreibt eine Erweiterung des STIX-Datenmodells.",
                        LevelId = lowLevel
                    }
                };

            foreach (Category item in categories)
            {
                Category? bestehendesItem = context.Category
                .FirstOrDefault(inDbSchonExestierend => inDbSchonExestierend.name == item.name);
                if (bestehendesItem == null)
                {
                    context.Category.Add(item);
                }
                else
                {
                    bestehendesItem.description = item.description;
                    bestehendesItem.LevelId = item.LevelId;
                }

            }


            await context.SaveChangesAsync();

        }
        catch (System.Exception)
        {

            throw;
        }


    }

}