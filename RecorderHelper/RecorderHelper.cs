using IMAPI2;
using System;
using System.Collections.Generic;
using System.Text;

namespace RecorderHelper
{
    /// <summary>
    /// Recorder Helper
    /// </summary>
    public class RecorderHelper
    {
        /// <summary>
        /// 获取光驱设备列表
        /// </summary>
        /// <returns></returns>
        public static List<Recorder> GetRecorderList()
        {
            List<Recorder> recordList = new List<Recorder>();

            // Create a DiscMaster2 object to connect to optical drives.
            MsftDiscMaster2 discMaster = new MsftDiscMaster2();
            for (int i = 0; i < discMaster.Count; i++)
            {
                if (discMaster[i] != null)
                {
                    Recorder recorder = new Recorder(discMaster[i]);
                    recordList.Add(recorder);
                }
            }
            return recordList;
        }
               
        /// <summary>
        /// 获取媒体类型描述
        /// </summary>
        /// <returns></returns>
        public static string GetMediaTypeName(IMAPI_MEDIA_PHYSICAL_TYPE mediaType)
        {
            string mediaTypeName = "";
            switch (mediaType)
            {
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDR:
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDRE:
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDROM:
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDR:
                    mediaTypeName = "CD-R";
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDROM:
                    mediaTypeName = "CD-ROM";
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDRW:
                    mediaTypeName = "CD-RW";
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DISK:
                    mediaTypeName = "Randomly-writable, hardware-defect managed media type " +
                        "that reports the \"Disc\" profile as current.";
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR:
                    mediaTypeName = "DVD-R";
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHRW:
                    mediaTypeName = "DVD-RW";
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR_DUALLAYER:
                    mediaTypeName = "DVD-R Dual Layer media";
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR:
                    mediaTypeName = "DVD+R";
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW:
                    mediaTypeName = "DVD+RW";
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW_DUALLAYER:
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR_DUALLAYER:
                    mediaTypeName = "DVD+R Dual Layer media";
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDRAM:
                    mediaTypeName = "DVD-RAM";
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDROM:
                    mediaTypeName = "Read-only DVD drive and/or disc";
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDR:
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDRAM:
                    break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDROM:
                    break;
                //case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_MAX:
                //  break;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_UNKNOWN:
                    mediaTypeName = "Empty device or an unknown disc type.";
                    break;
                default:
                    break;
            }
            return mediaTypeName;
        }

        /// <summary>
        /// 获取媒体状态描述
        /// </summary>
        /// <returns></returns>
        public static string GetMediaStateName(IMAPI_FORMAT2_DATA_MEDIA_STATE mediaState)
        {
            string mediaStateName = "unknown";
            switch (mediaState)
            {
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_APPENDABLE:
                    mediaStateName = "Appendable";
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_BLANK:
                    mediaStateName = "Blank";
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_DAMAGED:
                    mediaStateName = "Damaged";
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_ERASE_REQUIRED:
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_FINALIZED:
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_FINAL_SESSION:
                    mediaStateName = "Media is in final writing session.";
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_INFORMATIONAL_MASK:
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_NON_EMPTY_SESSION:
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_OVERWRITE_ONLY:
                    mediaStateName = "Currently, only overwriting is supported.";
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_UNKNOWN:
                    mediaStateName = "Media state is unknown.";
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_UNSUPPORTED_MASK:
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_UNSUPPORTED_MEDIA:
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_WRITE_PROTECTED:
                    break;
                default:
                    break;
            }
            return mediaStateName;
        }


        /// <summary>
        /// 获取媒体是否可刻录
        /// 实际使用中,值可能为6.直接刻录可行.不支持追加数据.
        /// 所以未列出的值,可以尝试刻录.刻录时,出异常了再说
        /// 刻坏了,换张盘๑乛◡乛๑
        /// </summary>
        /// <returns></returns>
        public static bool GetMediaBurnAble(IMAPI_FORMAT2_DATA_MEDIA_STATE mediaState)
        {
            bool burnAblue = true;
            switch (mediaState)
            {
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_APPENDABLE:
                    burnAblue = false;
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_BLANK:
                    burnAblue = true;
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_DAMAGED:
                    burnAblue = false;
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_ERASE_REQUIRED:
                    burnAblue = false;
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_FINALIZED:
                    burnAblue = false;
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_FINAL_SESSION:
                    burnAblue = false;
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_INFORMATIONAL_MASK:
                    burnAblue = false;
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_NON_EMPTY_SESSION:
                    burnAblue = false;
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_OVERWRITE_ONLY:
                    burnAblue = false;
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_UNKNOWN:
                    burnAblue = false;
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_UNSUPPORTED_MASK:
                    burnAblue = false;
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_UNSUPPORTED_MEDIA:
                    burnAblue = false;
                    break;
                case IMAPI_FORMAT2_DATA_MEDIA_STATE.IMAPI_FORMAT2_DATA_MEDIA_STATE_WRITE_PROTECTED:
                    burnAblue = false;
                    break;
                default:
                    break;
            }
            return burnAblue;
        }
    }
}
