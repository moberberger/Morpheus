namespace Morpheus
{
    public class WindowDragger
    {
        [System.Runtime.InteropServices.DllImport( "user32.dll" )]
        public static extern int SendMessage( IntPtr hWnd, int Msg, int wParam, int lParam );

        [System.Runtime.InteropServices.DllImport( "user32.dll" )]
        public static extern bool ReleaseCapture();

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private bool _isDragging;
        private IntPtr _handle;

        public event Action OnStartDragging;
        public event Action OnStopDragging;

        public WindowDragger( System.Windows.Forms.Form form )
        {
#pragma warning disable CA1416 // Validate platform compatibility
            _handle = form.Handle;
#pragma warning restore CA1416 // Validate platform compatibility
        }

        public bool IsDragging
        {
            get { return _isDragging; }
            set
            {
                if (value && !_isDragging)
                {
                    _isDragging = true;
                    OnStartDragging?.Invoke();

                    ReleaseCapture();
                    SendMessage( _handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0 );
                }
                if (!value && _isDragging)
                {
                    _isDragging = false;
                    OnStopDragging?.Invoke();
                }
            }
        }
    }
}
