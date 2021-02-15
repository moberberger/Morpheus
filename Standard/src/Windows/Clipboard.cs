using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

public static class Clipboard
{
    const uint cfUnicodeText = 13;

    public static void SetText( string text )
    {
        IntPtr hGlobal = default;

        OpenClipboard();
        EmptyClipboard();

        try
        {
            var byteCount = (text.Length + 1) * 2;

            hGlobal = Marshal.AllocHGlobal( byteCount );
            if (hGlobal == default) ThrowWin32();

            var target = GlobalLock( hGlobal );
            if (target == default) ThrowWin32();

            try
            {
                Marshal.Copy( text.ToCharArray(), 0, target, text.Length );
            }
            finally
            {
                GlobalUnlock( target );
            }

            if (SetClipboardData( cfUnicodeText, hGlobal ) == default) ThrowWin32();
            hGlobal = default;
        }
        finally
        {
            if (hGlobal != default)
                Marshal.FreeHGlobal( hGlobal );

            CloseClipboard();
        }
    }

    public static void OpenClipboard()
    {
        for (int i = 0; i < 10; i++)
        {
            if (OpenClipboard( default ))
                return;
            Thread.Sleep( 100 );
        }
        ThrowWin32();
    }

    [DllImport( "kernel32.dll", SetLastError = true )]
    static extern IntPtr GlobalLock( IntPtr hMem );

    [DllImport( "kernel32.dll", SetLastError = true )]
    [return: MarshalAs( UnmanagedType.Bool )]
    static extern bool GlobalUnlock( IntPtr hMem );

    [DllImport( "user32.dll", SetLastError = true )]
    [return: MarshalAs( UnmanagedType.Bool )]
    static extern bool OpenClipboard( IntPtr hWndNewOwner );

    [DllImport( "user32.dll", SetLastError = true )]
    [return: MarshalAs( UnmanagedType.Bool )]
    static extern bool CloseClipboard();

    [DllImport( "user32.dll", SetLastError = true )]
    static extern IntPtr SetClipboardData( uint uFormat, IntPtr data );

    [DllImport( "user32.dll" )]
    static extern bool EmptyClipboard();

    static void ThrowWin32() => throw new Win32Exception( Marshal.GetLastWin32Error() );
}