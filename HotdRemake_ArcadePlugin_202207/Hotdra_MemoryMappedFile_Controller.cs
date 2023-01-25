using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace HotdRemake_ArcadePlugin_202207
{
    public class Hotdra_MemoryMappedFile_Controller
    {
        #region WIN32

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFileMapping(int hFile, IntPtr lpAttributes, PageProtection flProtect, uint dwMaxSizeHi, uint dwMaxSizeLow, string lpName);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr OpenFileMapping(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, string lpName);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr MapViewOfFile(IntPtr hFileMapping, FileMapAccessType dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, uint dwNumberOfBytesToMap);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool UnmapViewOfFile(IntPtr pvBaseAddress);

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateMutex(IntPtr lpMutexAttributes, bool bInitialOwner, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);
        const UInt32 INFINITE = 0xFFFFFFFF;
        const UInt32 WAIT_ABANDONED = 0x00000080;
        const UInt32 WAIT_OBJECT_0 = 0x00000000;
        const UInt32 WAIT_TIMEOUT = 0x00000102;

        [DllImport("kernel32.dll")]
        public static extern bool ReleaseMutex(IntPtr hMutex);

        [Flags]
        public enum PageProtection : uint
        {
            NoAccess = 0x01,
            Readonly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            Guard = 0x100,
            NoCache = 0x200,
            WriteCombine = 0x400,
        }

        [Flags]
        public enum FileMapAccessType : uint
        {
            Copy = 0x01,
            Write = 0x02,
            Read = 0x04,
            AllAccess = 0x08,
            Execute = 0x20,
        }

        public const UInt32 ERROR_ALREADY_EXISTS = 183;
        public const Int32 INVALID_HANDLE_VALUE = -1;

        #endregion
        

        private IntPtr _hSharedMemoryFile = IntPtr.Zero;
        private String _sMmfName = String.Empty;
        private bool _bMmfAlreadyExist = false;
        private long _MemSize = 0;

        private IntPtr _Mutex;
        private String _sMutexName = String.Empty;

        private IntPtr _pwData = IntPtr.Zero;
        private bool _bInit = false;

        //Shared Memory Payload between DemulSHooter and the game, structured as followed:
        // Demulshooter --> Game :
        //Byte[0]-Byte[3]   : P1_X      [0-WindowWidth]
        //Byte[4]-Byte[7]   : P1_Y      [0-WindowHeight]
        //Byte[8]-Byte[11]  : P2_X      [0-WindowWidth]
        //Byte[12]-Byte[15] : P2_Y      [0-WindowHeight]
        //Byte[16]          : P1_Trigger        [0-1-2]
        //Byte[17]          : P2_Trigger        [0-1-2]
        //Byte[18]          : P1_Reload         [0-1]
        //Byte[19]          : P2_Reload         [0-1]
        // Game -> DemulShooter :
        //Byte[20]         : P1_StartLmp       [0-1]
        //Byte[21]         : P2_StartLmp       [0-1]
        //Byte[22]         : P1_Life           [int]
        //Byte[23]         : P2_Life           [int]
        //Byte[24]         : P1_Ammo           [int]
        //Byte[25]         : P2_Ammo           [int]
        //Byte[26]         : P1_Recoil         [0-1]
        //Byte[27]         : P2_Recoil         [0-1]
        //Byte[28]         : P1_Damaged        [int]
        //Byte[29]         : P2_Damaged        [int]
        //Byte[30]         : Credits           [int]

        private Byte[] _bPayload;

        #region Payload Indexes
        public const int PAYLOAD_LENGTH = 31;
        public const int PAYLOAD_INPUT_LENGTH = 20;
        public const int PAYLOAD_OUTPUTS_LENGTH = 11;

        public enum Payload_Inputs_Index
        {
            P1_AxisX = 0,
            P1_AxisY = 4,
            P2_AxisX = 8,
            P2_AxisY = 12,
            P1_Trigger = 16,
            P2_Trigger,
            P1_Reload,
            P2_Reload
        }

        public enum Payload_Outputs_Index
        {
            P1_StartLmp = PAYLOAD_INPUT_LENGTH,
            P2_StartLmp,            
            P1_Life,
            P2_Life,
            P1_Ammo,
            P2_Ammo,
            P1_Recoil,
            P2_Recoil,
            P1_Damaged,
            P2_Damaged,
            Credits
        }

        #endregion

        public Byte[] Payload
        {
            get { return _bPayload; }
            set { _bPayload = value; }
        }

        public bool IsOpened
        {
            get { return _bInit; }
        }

        public Hotdra_MemoryMappedFile_Controller(string MMF_Name, string Mutex_Name, long lngSize)
        {
            _sMmfName = MMF_Name;
            _sMutexName = Mutex_Name;

            if (lngSize <= 0 || lngSize > 0x00800000) lngSize = 0x00800000;
                _MemSize = lngSize;

            _bPayload = new Byte[PAYLOAD_LENGTH];
        }

        ~Hotdra_MemoryMappedFile_Controller()
        {
            MMFClose();
        }

        /// <summary>        
        ///  Initialize shared memory         
        /// </summary>        
        /// <param name="strName"> Shared memory name </param>        
        /// <param name="lngSize"> Shared memory size </param>        
        /// <returns></returns>        
        public int MMFOpen()
        {            
            if (_sMmfName.Length > 0)
            {
                // Create a memory share (INVALID_HANDLE_VALUE)                
                _hSharedMemoryFile = CreateFileMapping(INVALID_HANDLE_VALUE, IntPtr.Zero, PageProtection.ReadWrite, 0, (uint)_MemSize, _sMmfName);
                if (_hSharedMemoryFile == IntPtr.Zero)
                {
                    _bMmfAlreadyExist = false;
                    _bInit = false;
                    return 2; // Failed to create shared body					                 
                }
                else
                {
                    if (Marshal.GetLastWin32Error() == ERROR_ALREADY_EXISTS)  // Already Created   
                    {
                        _bMmfAlreadyExist = true;
                    }
                    else
                    {
                        _bMmfAlreadyExist = false; // New creation 
                    }
                }
                //---------------------------------------                
                // Create memory maps              
                //FileMapAccessType.AllAccess gives Memory protected ERROR !!!
                _pwData = MapViewOfFile(_hSharedMemoryFile, FileMapAccessType.Write, 0, 0, (uint)_MemSize);
                if (_pwData == IntPtr.Zero)
                {
                    _bInit = false;
                    CloseHandle(_hSharedMemoryFile);
                    return 3; // Failed to create memory map						                 
                }
                else
                {
                    if (_bMmfAlreadyExist == false)
                    {
                        // initialization  
                    }
                }
                //----------------------------------------  

                if (_sMutexName.Length > 0)
                {
                    // create IntPtrs for use with CreateMutex()
                    IntPtr ipMutexAttr = new IntPtr(0);
                    _Mutex = CreateMutex(ipMutexAttr, false, _sMutexName);                   
                
                if (_Mutex == IntPtr.Zero)                    
                        return 4; //Mutex Error 
                }
                else
                {
                    return 5; //Mutex Parameter Error
                }
            }
            else
            {
                return 1; // Parameter error 					                
            }
            _bInit = true;
                    
            return 0; // Create success         
        }

        /// <summary>        
        ///  Reading data         
        /// </summary>        
        /// <param name="bytData"> data </param>        
        /// <param name="lngAddr"> Initial address </param>        
        /// <param name="lngSize"> Number </param>        
        /// <returns></returns>        
        public int ReadAll()
        {
            if (PAYLOAD_LENGTH > _MemSize)
                return 2; // Beyond data area 

            if (_bInit)
            {
                if (_Mutex != null)
                    WaitForSingleObject(_Mutex, (uint)INFINITE);

                Marshal.Copy(_pwData, _bPayload, 0, (int)PAYLOAD_LENGTH);

                if (_Mutex != null)
                    ReleaseMutex(_Mutex);
            }
            else
            {
                return 1; // Shared memory not initialized             
            }
            return 0; // Read successfully         
        }

        public int Writeall()
        {
            return WriteByteArray(_bPayload, 0, 0, PAYLOAD_LENGTH);
        }

        public int WriteRecoilDirect(int PlayerRecoil)
        {
            return WriteByteArray(new byte[] { 0x01 }, 0, (int)Payload_Outputs_Index.P1_Recoil + PlayerRecoil, 1);        
        }

        public int WriteDamageDirect(int PlayerDamaged)
        {
            return WriteByteArray(new byte[] { 0x01 }, 0, (int)Payload_Outputs_Index.P1_Damaged + PlayerDamaged, 1);
        }

        /// <summary>        
        ///  Writing data         
        /// </summary>        
        /// <param name="bytData"> data </param>        
        /// <param name="lngAddr"> Initial address </param>        
        /// <param name="lngSize"> Number </param>        
        /// <returns></returns>        
        private int WriteByteArray(byte[] bytData, int lngAddr, int PayloadOffsetIndex, int lngSize)
        {
            if (lngAddr + lngSize > _MemSize)
                return 2; // Beyond data area             
            if (_bInit)
            {
                try
                {
                    if (_Mutex != null)
                        WaitForSingleObject(_Mutex, (uint)INFINITE);

                    Marshal.Copy(bytData, lngAddr, _pwData + PayloadOffsetIndex, lngSize);

                    if (_Mutex != null)
                        ReleaseMutex(_Mutex);
                }
                catch
                {
                    return 2;
                }
            }
            else
            {
                return 1; // Shared memory not initialized             
            }
            return 0; // Write a successful         
        }

        /// <summary>       
        ///  Turn off shared memory         
        /// </summary>        
        public void MMFClose()
        {
            if (_bInit)
            {
                UnmapViewOfFile(_pwData);
                CloseHandle(_hSharedMemoryFile);
            }
        }
    }
}
