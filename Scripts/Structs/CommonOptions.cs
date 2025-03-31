using JetBrains.Annotations;

namespace Sun {
    /// <summary>
    /// Common notification options such as title and text.
    /// </summary>
    public struct CommonOptions {
        [CanBeNull] public string Title;
        [CanBeNull] public string Text;

        /// <summary>
        /// Default empty notification options.
        /// </summary>
        public static readonly CommonOptions Default = new CommonOptions() {
            Title = null,
            Text = null
        };


        /// <summary>
        /// Overrides existing options with provided values.
        /// </summary>
        /// <param name="overrides">Optional new values to override the existing ones.</param>
        /// <returns>A new CommonOptions instance with applied overrides.</returns>
        public CommonOptions Override(CommonOptions? overrides) {
            CommonOptions result = this;

            if (overrides.HasValue) {
                CommonOptions overridesValue = overrides.Value;

                if (overridesValue.Title != null) {
                    result.Title = overridesValue.Title;
                }

                if (overridesValue.Text != null) {
                    result.Text = overridesValue.Text;
                }
            }

            return result;
        }
    }
}