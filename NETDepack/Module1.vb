Imports System.Reflection
Imports System.IO
Imports dnlib.DotNet
Imports dnlib.DotNet.Writer
Imports System.IO.Compression
Imports System.Runtime.InteropServices
Module Module1
    Private Declare Function SuspendThread Lib "kernel32.dll" (hThread As IntPtr) As UInteger
    Private Declare Function OpenThread Lib "kernel32.dll" (dwDesiredAccess As Integer, bInheritHandle As Boolean, dwThreadId As UInteger) As IntPtr
    Private Declare Auto Function CloseHandle Lib "kernel32" (handle As IntPtr) As Boolean
    Public Declare Function NtQueryInformationProcess Lib "ntdll.dll" (<Runtime.InteropServices.In()> ByVal ProcessHandle As IntPtr, <Runtime.InteropServices.In()> ByVal ProcessInformationClass As Integer, <System.Runtime.InteropServices.OutAttribute()> ByRef ProcessInformation As IntPtr, <Runtime.InteropServices.In()> ByVal ProcessInformationLength As Integer, <Runtime.InteropServices.Optional()> <System.Runtime.InteropServices.OutAttribute()> ByRef ReturnLength As Integer) As Integer
    Public Function GetCurrentModule(ByVal [module] As ModuleDefMD) As Byte()
        Dim memoryStream As MemoryStream = New MemoryStream()
        If [module].IsILOnly Then
            [module].Write(memoryStream, New ModuleWriterOptions([module]))
        Else
            [module].NativeWrite(memoryStream, New NativeModuleWriterOptions([module], False))

        End If
        Dim array As Byte() = New Byte(memoryStream.Length - 1) {}
        memoryStream.Position = 0L
        memoryStream.Read(array, 0, CInt(memoryStream.Length))
        Return array
    End Function

    Sub Main(ByVal args As String())
        If args.Count = 1 Then
            unpack(args(0))
            'Dim asm As Assembly = Assembly.LoadFile(args(0))
            'For x = 0 To asm.GetManifestResourceNames.Count - 1
            'Dim moduleDefMD As ModuleDefMD = moduleDefMD.Load(args(0))
            'Dim entryPoint As MethodDef = moduleDefMD.EntryPoint
            'Dim ilasByteArray As Byte() = Assembly.Load(GetCurrentModule(moduleDefMD)).ManifestModule.ResolveMethod(entryPoint.MDToken.ToInt32()).GetMethodBody().GetILAsByteArray()
            'Dim ms As New MemoryStream()
            'asm.GetManifestResourceStream(asm.GetManifestResourceNames(x)).CopyTo(ms)
            'Decrypt(ms.ToArray, ilasByteArray)
            'Next
        End If
    End Sub

    Sub unpack(ByVal filepath As String)
        Dim asm = System.Reflection.Assembly.LoadFile(filepath)
        Dim [structure] As IntPtr = New IntPtr(0)
        Dim [module] As [Module] = New StackTrace().GetFrame(0).GetMethod().[Module]
        Dim pr As New ProcessStartInfo(filepath)
        pr.WindowStyle = ProcessWindowStyle.Hidden
#Region "Code from JIT Freezer"
        Dim pr_ As Process = Process.Start(pr)
        For Each obj As Object In pr_.Threads
            Dim processThread As ProcessThread = CType(obj, ProcessThread)
            Dim intPtr As IntPtr = OpenThread(2, False, CUInt(processThread.Id))
            If Not (intPtr = IntPtr.Zero) Then
                SuspendThread(intPtr)
                CloseHandle(intPtr)
            End If
        Next
#End Region
        Dim num As Integer
        NtQueryInformationProcess(pr_.Handle, 7, [structure], Marshal.SizeOf([structure]), num)
        Dim manifestResourceStream As Stream
        For x = 0 To asm.GetManifestResourceNames.Count - 1
            manifestResourceStream = asm.GetManifestResourceStream(asm.GetManifestResourceNames(x))
            Dim array As Byte() = New Byte(manifestResourceStream.Length + CLng(CInt([structure])) - 1) {}
            manifestResourceStream.Read(array, CInt([structure]), array.Length + CInt([structure]))
            NtQueryInformationProcess(pr_.Handle, 7, [structure], Marshal.SizeOf([structure]), num)
            Dim ilasByteArray As Byte() = [module].Assembly.ManifestModule.ResolveMethod(100663298 + CInt([structure])).GetMethodBody().GetILAsByteArray()
            array = Decrypt(array, ilasByteArray)
            File.WriteAllBytes(asm.GetManifestResourceNames(x) & ".dmp", array)
        Next
        'Dim assembly As Assembly = assembly.Load(array)
        'NtQueryInformationProcess(Process.GetCurrentProcess().Handle, 7, [structure], Marshal.SizeOf([structure]), num)
        'Dim entryPoint As MethodInfo = assembly.EntryPoint
        'Dim obj As Object = assembly.CreateInstance(entryPoint.ToString())
        'entryPoint.Invoke(obj, Nothing)
    End Sub

    Public Function Decrypt(plain As Byte(), Key As Byte()) As Byte()
        For i As Integer = 0 To 5 - 1
            For j As Integer = 0 To plain.Length - 1
                plain(j) = plain(j) Xor Key(j Mod Key.Length)
                For k As Integer = 0 To Key.Length - 1
                    plain(j) = CByte((CInt(plain(j)) Xor (CInt(Key(k)) << i Xor k) + j))
                Next
            Next
        Next
        Return plain
    End Function

    Public Function Decompress(ByVal data As Byte()) As Byte()
        Dim stream As MemoryStream = New MemoryStream(data)
        Dim memoryStream As MemoryStream = New MemoryStream()
        Using deflateStream As DeflateStream = New DeflateStream(stream, CompressionMode.Decompress)
            deflateStream.CopyTo(memoryStream)
        End Using
        Return memoryStream.ToArray()
    End Function
End Module
