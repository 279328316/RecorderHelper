using IMAPI2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RecorderHelper
{
    /// <summary>
    /// 刻录进度delegate
    /// </summary>
    public delegate void BurnProgressChanged(BurnProgress burnProgress);

    /// <summary>
    /// Recorder 对象
    /// </summary>
    public class Recorder
    {
        private string uniqueId = null; //标识Id
        MsftDiscRecorder2 msRecorder = null;    //Recorder

        #region Properties

        /// <summary>
        /// 当前磁盘标签
        /// </summary>
        public string RecorderName { get; private set; }

        /// <summary>
        /// 是否支持当前刻录机
        /// </summary>
        public bool IsRecorderSupported { get; private set; }

        /// <summary>
        /// 是否支持当前磁盘媒体
        /// </summary>
        public bool IsCurrentMediaSupported { get; private set; }

        /// <summary>
        /// 当前磁盘可用大小
        /// </summary>
        public long FreeDiskSize { get; private set; }

        /// <summary>
        /// 当前磁盘总大小
        /// </summary>
        public long TotalDiskSize { get; private set; }

        /// <summary>
        /// 当前媒体状态
        /// </summary>
        public IMAPI_FORMAT2_DATA_MEDIA_STATE CurMediaState { get; private set; }

        /// <summary>
        /// 当前媒体状态
        /// </summary>
        public string CurMediaStateName { get; private set; }

        /// <summary>
        /// 当前媒体类型
        /// </summary>
        public IMAPI_MEDIA_PHYSICAL_TYPE CurMediaType { get; private set; }

        /// <summary>
        /// 当前媒体类型
        /// </summary>
        public string CurMediaTypeName { get; private set; }

        /// <summary>
        /// 是否可以刻录
        /// </summary>
        public bool CanBurn {get;private set;}

        /// <summary>
        /// 待刻录媒体对象List
        /// </summary>
        public List<BurnMedia> BurnMediaList {get;set;}

        /// <summary>
        /// 待刻录媒体文件大小
        /// </summary>
        public long BurnMediaFileSize { get; set; }

        /// <summary>
        /// 刻录进度变化通知
        /// </summary>
        public BurnProgressChanged OnBurnProgressChanged { get; set; }
        #endregion

        /// <summary>
        /// Recorder Ctor
        /// </summary>
        /// <param name="uniqueId">标识Id</param>
        public Recorder(string uniqueId)
        {
            this.uniqueId = uniqueId;
            msRecorder = new MsftDiscRecorder2();
            msRecorder.InitializeDiscRecorder(uniqueId);
            InitRecorder();

            this.BurnMediaList = new List<BurnMedia>();
            this.BurnMediaFileSize = 0;
        }

        /// <summary>
        /// 初始化Recorder
        /// 更新Recorder信息,更新光盘后可重试.
        /// </summary>
        public void InitRecorder()
        {
            try
            {
                if (msRecorder.VolumePathNames != null && msRecorder.VolumePathNames.Length > 0)
                {
                    foreach (object mountPoint in msRecorder.VolumePathNames)
                    {   //挂载点 取其中一个
                        RecorderName = mountPoint.ToString();
                        break;
                    }
                }
                // Define the new disc format and set the recorder
                MsftDiscFormat2Data dataWriter = new MsftDiscFormat2Data();
                dataWriter.Recorder = msRecorder;

                if (!dataWriter.IsRecorderSupported(msRecorder))
                {
                    return;
                }
                if (!dataWriter.IsCurrentMediaSupported(msRecorder))
                {
                    return;
                }
                if (dataWriter.FreeSectorsOnMedia >= 0)
                {
                    FreeDiskSize = dataWriter.FreeSectorsOnMedia * 2048L;
                }

                if (dataWriter.TotalSectorsOnMedia >= 0)
                {
                    TotalDiskSize = dataWriter.TotalSectorsOnMedia * 2048L;
                }
                CurMediaState = dataWriter.CurrentMediaStatus;
                CurMediaStateName = RecorderHelper.GetMediaStateName(CurMediaState);
                CurMediaType = dataWriter.CurrentPhysicalMediaType;
                CurMediaTypeName = RecorderHelper.GetMediaTypeName(CurMediaType);
                CanBurn = RecorderHelper.GetMediaBurnAble(CurMediaState);
            }
            catch (COMException ex)
            {
                //string errorMsg = IMAPIReturnValues.GetName(ex.ErrorCode);
                string errMsg = ex.Message.Replace("\r\n", ""); //去掉异常信息里的\r\n
                this.CurMediaStateName = $"COM Exception:{errMsg}";
                //throw new Exception($"The following COM Exception occured:{ex.Message}-{errorMsg}");
            }
            catch (Exception ex)
            {
                this.CurMediaStateName = $"{ex.Message}";
            }
        }

        /// <summary>
        /// 添加刻录媒体对象
        /// </summary>
        public BurnMedia AddBurnMedia(string path)
        {
            BurnMedia media = null;
            if(string.IsNullOrEmpty(path))
            {
                throw new Exception("文件路径不能为空.");
            }
            if(!CanBurn)
            {
                throw new Exception("当前磁盘状态不支持刻录.");
            }
            media = new BurnMedia();
            long fileSize = 0;
            if (Directory.Exists(path))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                fileSize = GetDirectorySize(path);
                media.MediaName = dirInfo.Name;
                media.MediaPath = dirInfo.FullName;
                media.Size = fileSize;
                media.IsDirectory = true;
            }
            else if (File.Exists(path))
            {
                FileInfo fileInfo = new FileInfo(path);
                fileSize = fileInfo.Length;
                media.MediaName = fileInfo.Name;
                media.MediaPath = fileInfo.FullName;
                media.Size = fileSize;
                media.IsDirectory = false;
            }
            else
            {
                throw new Exception("文件不存在");
            }
            if (BurnMediaFileSize + fileSize >= FreeDiskSize)
            {
                throw new Exception("剩余空间不足");
            }
            if (BurnMediaList.Any(m => m.MediaName.ToLower() == media.MediaName.ToLower()))
            {
                throw new Exception($"已存在媒体名称为{media.MediaName}的对象");
            }
            // Add the directory and its contents to the file system 
            BurnMediaList.Add(media);
            BurnMediaFileSize += fileSize;
            return media;
        }

        /// <summary>
        /// 刻录
        /// </summary>
        public void Burn(string diskName = "SinoUnion")
        {
            if(!CanBurn)
            {
                throw new Exception("当前磁盘状态不支持刻录");
            }
            if (string.IsNullOrEmpty(diskName))
            {
                throw new Exception("DiskName不能为空");
            }
            if (BurnMediaList.Count <= 0)
            {
                throw new Exception("待刻录文件列表不能为空");
            }
            if(BurnMediaFileSize<=0)
            {
                throw new Exception("待刻录文件大小为0");
            }

            try
            {   //说明
                //1.fsi.ChooseImageDefaults用的是IMAPI2FS的,我们定义的msRecorder是IMAPI2的.所以必须用动态类型
                //2.dataWriter也要使用动态类型,要不然Update事件会出异常.
                // Create an image stream for a specified directory.
                dynamic fsi = new IMAPI2FS.MsftFileSystemImage();  // Disc file system
                IMAPI2FS.IFsiDirectoryItem dir = fsi.Root;                 // Root directory of the disc file system                
                dynamic dataWriter = new MsftDiscFormat2Data(); //Create the new disc format and set the recorder

                dataWriter.Recorder = msRecorder;
                dataWriter.ClientName = "SinoGram";
                //不知道这方法不用行不行.用的参数是IMAPI2FS的.
                //所以学官网的例子,把fsi改成了动态的.使用msRecorder作为参数
                fsi.ChooseImageDefaults(msRecorder);

                //设置相关信息
                fsi.VolumeName = diskName;   //刻录磁盘名称
                for (int i = 0; i < BurnMediaList.Count; i++)
                {
                    dir.AddTree(BurnMediaList[i].MediaPath, true);
                }
                // Create an image from the file system
                IStream stream = fsi.CreateResultImage().ImageStream;
                try
                {
                    dataWriter.Update += new DDiscFormat2DataEvents_UpdateEventHandler(BurnProgressChanged);
                    dataWriter.Write(stream);// Write stream to disc
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    if (stream != null)
                    {
                        Marshal.FinalReleaseComObject(stream);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刻录失败:{ex.Message}");
            }
        }

        /// <summary>
        /// 刻录进度通知
        /// </summary>
        /// <param name="object"></param>
        /// <param name="progress"></param>
        void BurnProgressChanged(dynamic @object, dynamic progress)
        {
            BurnProgress burnProgress = new BurnProgress();
            try
            {
                burnProgress.ElapsedTime = progress.ElapsedTime;
                burnProgress.TotalTime = progress.TotalTime;
                burnProgress.CurrentAction = progress.CurrentAction;
                if (burnProgress.ElapsedTime > burnProgress.TotalTime)
                {   //如果已用时间已超过预估总时间.则将预估总时间设置为已用时间
                    burnProgress.TotalTime = burnProgress.ElapsedTime;
                }
                string strTimeStatus;
                strTimeStatus = "Time: " + progress.ElapsedTime + " / " + progress.TotalTime;
                int currentAction = progress.CurrentAction;
                switch (currentAction)
                {
                    case (int)IMAPI2.IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_CALIBRATING_POWER:
                        burnProgress.CurrentActionName = "Calibrating Power (OPC)";
                        break;
                    case (int)IMAPI2.IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_COMPLETED:
                        burnProgress.CurrentActionName = "Completed the burn";
                        burnProgress.Percent = 1;
                        burnProgress.TotalTime = burnProgress.ElapsedTime;  //刻录完成,将预估用时,修正为已用时间
                        break;
                    case (int)IMAPI2.IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_FINALIZATION:
                        burnProgress.CurrentActionName = "Finishing the writing";
                        burnProgress.Percent = 1;
                        burnProgress.TotalTime = burnProgress.ElapsedTime;  //写入完成,将预估用时,修正为已用时间
                        break;
                    case (int)IMAPI2.IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_FORMATTING_MEDIA:
                        burnProgress.CurrentActionName = "Formatting media";
                        break;
                    case (int)IMAPI2.IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_INITIALIZING_HARDWARE:
                        burnProgress.CurrentActionName = "Initializing Hardware";
                        break;
                    case (int)IMAPI2.IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_VALIDATING_MEDIA:
                        burnProgress.CurrentActionName = "Validating media";
                        break;
                    case (int)IMAPI2.IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_VERIFYING:
                        burnProgress.CurrentActionName = "Verifying the data";
                        break;
                    case (int)IMAPI2.IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_WRITING_DATA:
                        dynamic totalSectors;
                        dynamic writtenSectors;
                        dynamic startLba;
                        dynamic lastWrittenLba;
                        totalSectors = progress.SectorCount;
                        startLba = progress.StartLba;
                        lastWrittenLba = progress.LastWrittenLba;
                        writtenSectors = lastWrittenLba - startLba;                        
                        burnProgress.CurrentActionName = "Writing data";
                        burnProgress.Percent = Convert.ToDecimal(writtenSectors)/ Convert.ToDecimal(totalSectors);
                        break;
                    default:
                        burnProgress.CurrentActionName = "Unknown action";
                        break;
                }
            }
            catch (Exception ex)
            {
                burnProgress.CurrentActionName = ex.Message;
            }
            if (OnBurnProgressChanged != null)
            {
                OnBurnProgressChanged(burnProgress);
            }
        }


        /// <summary>
        /// 获取指定路径的大小
        /// </summary>
        /// <param name="dirPath">路径</param>
        /// <returns></returns>
        private long GetDirectorySize(string dirPath)
        {
            long len = 0;
            //判断该路径是否存在（是否为文件夹）
            if (!Directory.Exists(dirPath))
            {
                if (File.Exists(dirPath))
                {
                    //查询文件的大小
                    FileInfo fileInfo = new FileInfo(dirPath);
                    len = fileInfo.Length;
                }
            }
            else
            {
                //定义一个DirectoryInfo对象
                DirectoryInfo di = new DirectoryInfo(dirPath);

                //通过GetFiles方法，获取di目录中的所有文件的大小
                foreach (FileInfo fi in di.GetFiles())
                {
                    len += fi.Length;
                }
                //获取di中所有的文件夹，并存到一个新的对象数组中，以进行递归
                DirectoryInfo[] dis = di.GetDirectories();
                if (dis.Length > 0)
                {
                    for (int i = 0; i < dis.Length; i++)
                    {
                        len += GetDirectorySize(dis[i].FullName);
                    }
                }
            }
            return len;
        }
    }
}
