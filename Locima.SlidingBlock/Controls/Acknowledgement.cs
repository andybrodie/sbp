namespace Locima.SlidingBlock.Controls
{
    /// <summary>
    /// Represents a single acknowledgement within the <see cref="Acknowledgements"/> page.
    /// </summary>
    public class Acknowledgement
    {

        /// <summary>
        /// The title of the acknowledgement, i.e. the software used or person credited
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Description of why this acknowledgement exists
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A link to display under the acknowledgement to take the user to a related resource
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// The author of code that the acknowledgement relates to
        /// </summary>
        public string Author { get; set; }
    }
}