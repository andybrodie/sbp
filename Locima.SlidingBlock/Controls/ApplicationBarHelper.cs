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
        ///   This means that all the actual URLs are in one place rather than distributed throughout the application, making them easier to manage.
        /// </remarks>
        /// <see cref="GetIconUri"/>
        public static readonly Dictionary<string, Uri> ButtonIcons = new Dictionary<string, Uri>
            {
                {"Cancel", GetIconUri("appbar.cancel.rest.png")},
                {"Tick", GetIconUri("appbar.check.rest.png")},
                {"Pause", GetIconUri("appbar.transport.pause.rest.png")},
                {"Resume", GetIconUri("appbar.transport.play.rest.png")},
                {"Save", GetIconUri("appbar.save.rest.png")},
                {"New", GetIconUri("appbar.new.rest.png")},
                {"Autosolve", GetIconUri("appbar.transport.ff.rest.png")},
                {"Edit", GetIconUri("appbar.edittext.rest.png")},
                {"Copy", GetIconUri("appbar.tabs.rest.png")}
            };

        /// <summary>
        /// Convience method used in the creation of <see ccref="ButtonIcons"/> to "force" all icons used by <see cref="IApplicationBarIconButton"/> are in the same place
        /// </summary>
        /// <param name="filename">The base filename of the icon image file (no path)</param>
        /// <returns>The <see cref="Uri"/> that the image is obtainable from</returns>
        private static Uri GetIconUri(string filename)
        {
            return new Uri("/Icons/" + filename, UriKind.Relative);
        }


        /// <summary>
        /// Convenience method to create an <see cref="IApplicationBarMenuItem"/>
        /// </summary>
        /// <param name="text">The text to use</param>
        /// <returns>The created <see cref="IApplicationBarMenuItem"/></returns>
        public static IApplicationBarMenuItem CreateMenuItem(string text)
        {
            return new ApplicationBarMenuItem
                {
                    Text = text
                };
        }


        /// <summary>
        /// Convenience method to create an application bar button
        /// </summary>
        /// <param name="iconUri">The icon to use for the button</param>
        /// <param name="text">The text label (must already be localized) to use for the button</param>
        /// <returns>A button to add to an <see cref="IApplicationBar"/></returns>
        public static IApplicationBarIconButton CreateButton(Uri iconUri, string text)
        {
            return new ApplicationBarIconButton
                {
                    IconUri = iconUri,
                    Text = text
                };
        }


        /// <summary>
        /// Creates an <see cref="IApplicationBarIconButton"/> using <see cref="CreateButton"/> and adds it to the <paramref name="applicationBar"/>.
        /// </summary>
        /// <param name="applicationBar">The application bar to add the button to</param>
        /// <param name="iconUri">The icon to use for the button</param>
        /// <param name="text">The text label (must already be localized) to use for the button</param>
        /// <returns>The button that was added to the <paramref name="applicationBar"/></returns>
        public static IApplicationBarIconButton AddButton(IApplicationBar applicationBar, Uri iconUri,
                                                          string text)
        {
            IApplicationBarIconButton button = CreateButton(iconUri, text);
            applicationBar.Buttons.Add(button);
            return button;
        }


        /// <summary>
        /// Creates an <see cref="IApplicationBarMenuItem"/> using <see cref="CreateMenuItem"/> and adds it to the <paramref name="applicationBar"/>.
        /// </summary>
        /// <param name="applicationBar">The application bar to add the menu item to</param>
        /// <param name="text">The text label (must already be localized) to use for the menu item</param>
        /// <returns>The menu item that was added to the <paramref name="applicationBar"/></returns>
        public static IApplicationBarMenuItem AddMenuItem(IApplicationBar applicationBar, string text)
        {
            IApplicationBarMenuItem item = CreateMenuItem(text);
            applicationBar.MenuItems.Add(item);
            return item;
        }
    }
}