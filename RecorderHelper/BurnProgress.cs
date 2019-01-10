using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecorderHelper
{
    /// <summary>
    /// 刻录进度对象
    /// </summary>
    public class BurnProgress
    {
        /// <summary>
        /// 当前操作
        /// 对应IMAPI2.IMAPI_FORMAT2_DATA_WRITE_ACTION枚举
        /// 4 正在写入数据 5完成数据写入 6 刻录完成
        /// </summary>
        public int CurrentAction { get; set; }

        /// <summary>
        /// 当前操作Name
        /// </summary>
        public string CurrentActionName { get; set; }

        /// <summary>
        /// 已用时间单位S
        /// </summary>
        public int ElapsedTime { get; set; }

        /// <summary>
        /// 预计总时间单位S
        /// </summary>
        public int TotalTime { get; set; }

        /// <summary>
        /// 数据写入进度
        /// </summary>
        public decimal Percent { get; set; }

        /// <summary>
        /// 数据写入进度%
        /// </summary>
        public string PercentStr { get { return Percent.ToString("0.00%"); } }
    }
}
