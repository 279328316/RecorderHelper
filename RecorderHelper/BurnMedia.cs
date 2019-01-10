using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecorderHelper
{
    /// <summary>
    /// 刻录媒体
    /// </summary>
    public class BurnMedia
    {
        /// <summary>
        /// 路径
        /// </summary>
        public string MediaPath { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string MediaName { get; set; }

        /// <summary>
        /// 是否是文件夹
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        public long Size { get; set; }
    }
}
