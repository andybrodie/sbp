using System;
using System.Collections.Generic;
using Locima.SlidingBlock.Scrambles;

namespace Locima.SlidingBlock.GameTemplates.SinglePlayer
{
    public class SinglePlayerGame
    {
        private const string TemplatePath = "/GameTemplates/SinglePlayer/";

        public static GameDefinition Create()
        {
            const string andy = "Andy Brodie";
            Uri andyLink = new Uri("http://www.locima.co.uk");
            LicenseDefinition ccby30 = new LicenseDefinition
                {
                    Link = new Uri("http://ccby3.0.org/licenses/by/3.0/"),
                    Title = "Creative Commons Attribution 3.0 (CC BY 3.0)"
                };


            GameDefinition gm = new GameDefinition
                {
                    Levels = new List<LevelDefinition>(3)
                        {
                            new LevelDefinition
                                {
                                    XapImageUri = new Uri(TemplatePath + "1.jpg", UriKind.Relative),
                                    ImageTitle = "Dax the Labrador",
                                    ImageText =
                                        "This is some description text about the picture, can include <a href=\"http://www.flickr.com\"> links to the source of the picture",
                                    OwnerName = andy,
                                    OwnerUri = andyLink,
                                    License = ccby30,
                                    ScrambleType = Scrambler.ScrambleType.OneMoveToFinish
                                },
                            new LevelDefinition
                                {
                                    XapImageUri = new Uri(TemplatePath + "2.jpg", UriKind.Relative),
                                    ImageTitle = "Roman ruin",
                                    ImageText = "A holiday snap from when I went to Rome in 2010",
                                    OwnerName = andy,
                                    OwnerUri = andyLink,
                                    License = ccby30,
                                    ScrambleType = Scrambler.ScrambleType.OneMoveToFinish
                                },
                            new LevelDefinition
                                {
                                    XapImageUri = new Uri(TemplatePath + "3.jpg", UriKind.Relative),
                                    ImageTitle = "Flower",
                                    ImageText =
                                        "A flower in Exbury Gardens, Hampshire",
                                    OwnerName = andy,
                                    OwnerUri = andyLink,
                                    License = ccby30,
                                    ScrambleType = Scrambler.ScrambleType.OneMoveToFinish
                                }
                        }
                };

            return gm;
        }
    }
}