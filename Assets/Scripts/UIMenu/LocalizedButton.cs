using UnityEngine.UIElements;

namespace UI.Menu 
{
    [UxmlElement]
    public partial class LocalizedButton : Button
    {
        public static BindingId keyProperty = nameof(key);

        [UxmlAttribute] public string key;

        public LocalizedButton() 
        {
        
        }
    }
}
