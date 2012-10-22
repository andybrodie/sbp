using System;
using System.Collections.Generic;
using Microsoft.Phone.Shell;

namespace Locima.SlidingBlock.Controls
{
    /// <summary>
    ///   Simple helper class to make it a bit easier to create <see cref="ApplicationBar" /> objects.
    /// </summary>
    /// <remarks>
    ///   <para> All <see cref="ApplicationBar" /> instance in this application have to be created in code because XAML cannot support the creation of localized text on <see
    ///    cref="IApplicationBarIconButton" /> and <see cref="IApplicationBarMenuItem" /> </para>
    /// </remarks>
    public class ApplicationBarHelper
    {
        /// <summary>
        ///   A collection of all the application bar icons that are available for use within <see cref="IApplicationBarIconButton" /> mapped to a convenient name.
        /// </summary>
        /// <remarks>
        ///   This means that all the actual URLs are in one place rather than distributed throughout the application, making them easier to manage
        /// </remarks>
        public static readonly Dictionary<string, Uri> Buttons = new Dictionary<string, Uri>
            {
                {"Cancel", GetIconUri("appbar.cancel.rest.png")},
                {"Tick", GetIconUri("appbar.check.rest.png")},
                {"Pause", GetIconUri("appbar.transport.pause.rest.png")},
                {"Resume", GetIconUri("appbar.transport.play.rest.png")},
                {"Save", GetIconUri("appbar.save.rest.png")},
                {"New", GetIconUri("appbar.new.rest.png")},
                {"Autosolve", GetIconUri("appbar.transport.ff.rest.png")}
            };

        private static Uri GetIconUri(string filename)
        {
            return new Uri("/Icons/" + filename, UriKind.Relative);
        }


        public static IApplicationBarMenuItem CreateMenuItem(string menuLabel)
        {
            return new ApplicationBarMenuItem
                {
                    Text = menuLabel
                };
        }


        public static IApplicationBarIconButton CreateButton(Uri iconUri, string labelName)
        {
            return new ApplicationBarIconButton
                {
                    IconUri = iconUri,
                    Text = labelName
                };
        }


        public static IApplicationBarIconButton AddButton(IApplicationBar applicationBar, Uri buttonImageUri,
                                                          string textLabel)
        {
            IApplicationBarIconButton button = CreateButton(buttonImageUri, textLabel);
            applicationBar.Buttons.Add(button);
            return button;
        }


        public static IApplicationBarMenuItem AddMenuItem(IApplicationBar applicationBar, string textLabel)
        {
            IApplicationBarMenuItem item = CreateMenuItem(textLabel);
            applicationBar.MenuItems.Add(item);
            return item;
        }
    }
}