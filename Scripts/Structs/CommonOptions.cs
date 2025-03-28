using JetBrains.Annotations;

namespace Sun {
    public struct CommonOptions {
        [CanBeNull] public string Title;
        [CanBeNull] public string Text;

        public static readonly CommonOptions Default = new CommonOptions() {
            Title = null,
            Text = null
        };

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