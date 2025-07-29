namespace Game.UI.Foundation.Editor
{
    public class UIToolBarLogic<T> where T : class, new()
    {
        protected static T _Instance;
        
        private static readonly object sysLock = new object();

        public static T S
        {
            get
            {
                if (_Instance == null)
                {
                    lock (sysLock)
                    {
                        if (_Instance == null)
                        {
                            _Instance = new T();
                        }
                    }
                }
                return _Instance;
            }
        }
        
        public virtual void Open()
        {

        }

        public virtual void Close()
        {

        }
    }
}