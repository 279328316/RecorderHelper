using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RecorderHelper.ConsoleApp
{
    class Program
    {
        static ConsoleColor colorBack = Console.BackgroundColor;
        static ConsoleColor colorFore = Console.ForegroundColor;
        static void Main(string[] args)
        {
            while (true)
            {
                try
                {
                    #region 查找光驱设备
                    Console.Clear();
                    Console.WriteLine("正在查找光驱设备..");
                    List<Recorder> recorderList = RecorderHelper.GetRecorderList();
                    if (recorderList.Count <= 0)
                    {
                        Console.WriteLine("没有可以使用的光驱,请检查.");
                        Console.WriteLine("请连接光驱后,按任意键重试...");
                        Console.ReadKey();
                        continue;
                    }
                    for (int i = 0; i < recorderList.Count; i++)
                    {
                        Recorder tempRecorder = recorderList[i];
                        Console.WriteLine($"发现光驱设备:[{i+1}]  {tempRecorder.RecorderName}");
                        Console.WriteLine($"媒体类型:{tempRecorder.CurMediaTypeName}");
                        Console.WriteLine($"媒体状态:{tempRecorder.CurMediaStateName}");
                        Console.WriteLine("支持刻录:" + (tempRecorder.CanBurn ? "√" : "×"));
                        Console.WriteLine($"可用大小:{FormatFileSize(tempRecorder.FreeDiskSize)}");
                        Console.WriteLine($"总大小:{FormatFileSize(tempRecorder.TotalDiskSize)}");
                    }
                    if (!recorderList.Any(r=>r.CanBurn))
                    {
                        Console.WriteLine("没有可以用于刻录的光驱设备,请检查后,按任意键重试.");
                        Console.ReadKey();
                        continue;
                    }
                    #endregion

                    #region 选择刻录使用的光驱
                    Recorder recorder;
                    if (recorderList.Count > 1)
                    {
                        Console.WriteLine("发现多个光驱设备,请输入序号选择刻录使用的光驱.");
                        int recorderIndex = -1;
                        while (recorderIndex == -1)
                        {   //tempIndex 1开始
                            int tempIndex = -1;
                            string recorderIndexStr = Console.ReadLine();
                            int.TryParse(recorderIndexStr, out tempIndex);
                            if (tempIndex - 1 < 0 || tempIndex > recorderList.Count)
                            {
                                Console.WriteLine("输入序号无效,请重新输入.");
                                continue;
                            }
                            if (!recorderList[tempIndex-1].CanBurn)
                            {
                                Console.WriteLine("当前设备状态异常,不能进行刻录,请选择其它设备.");
                                continue;
                            }
                            recorderIndex = tempIndex -1;
                        }
                        recorder = recorderList[recorderIndex];
                    }
                    else
                    {
                        recorder = recorderList[0];
                    }
                    Console.WriteLine($"使用设备[{recorder.RecorderName}  {recorder.CurMediaTypeName}]进行刻录");
                    #endregion

                    #region 添加刻录文件
                    while (true)
                    {
                        Console.WriteLine("添加文件:请输入待刻录文件或文件夹路径. 0完成 1查看已添加文件");
                        string filePath = Console.ReadLine();
                        if (string.IsNullOrEmpty(filePath))
                        {
                            continue;
                        }
                        else if (filePath == "0")
                        {
                            break;
                        }
                        else if (filePath == "1")
                        {
                            ShowBurnMediaListInfo(recorder);
                        }
                        else
                        {
                            try
                            {
                                BurnMedia media = recorder.AddBurnMedia(filePath);
                                Console.WriteLine($"添加成功:{filePath}");
                                Console.WriteLine("文件大小:" + FormatFileSize(media.Size));
                                Console.WriteLine("已添加文件总大小:" + FormatFileSize(recorder.BurnMediaFileSize));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"添加失败:{ex.Message}");
                            }
                        }
                    }
                    #endregion

                    #region 刻录文件

                    if (recorder.BurnMediaList.Count <= 0)
                    {
                        Console.WriteLine($"未添加任何刻录文件.已退出刻录过程.");
                    }
                    else
                    {
                        #region 刻录前确认
                        bool confirmBurn = false;
                        Console.Clear();
                        ShowBurnMediaListInfo(recorder);
                        while (true)
                        {
                            Console.WriteLine();
                            Console.ForegroundColor = ConsoleColor.DarkGreen;//设置颜色.
                            Console.WriteLine($"刻录过程一旦开始,终止可能会造成磁盘损坏.确认要开始刻录(y/n)?");
                            Console.ForegroundColor = colorFore;//还原颜色.
                            string confirmStr = Console.ReadLine();
                            if (confirmStr.ToLower() == "n")
                            {
                                break;
                            }
                            else if (confirmStr.ToLower() == "y")
                            {
                                confirmBurn = true;
                                break;
                            }
                        }
                        if (!confirmBurn)
                        {
                            Console.WriteLine($"本次刻录过程已退出");
                            continue;
                        }
                        #endregion
                        Console.CursorVisible = false;    //隐藏光标
                        ShowBurnProgressChanged(recorder);
                        recorder.Burn();    //刻录
                        Console.WriteLine();
                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"异常:{ex.Message}\r\n{ex.StackTrace}");
                }
                Console.WriteLine($"按任意键退出...");
                Console.ReadKey();
            }
        }


        /// <summary>
        /// 返回带单位的文件大小 取部分
        /// </summary>
        /// <param name="fileSize">字节数</param>
        /// <returns></returns>
        static string FormatFileSize(long fileSize)
        {
            if (fileSize < 0)
            {
                throw new ArgumentOutOfRangeException("fileSize");
            }
            else if (fileSize >= 1024 * 1024 * 1024)
            {
                return string.Format("{0:########0.00} GB", ((Double)fileSize) / (1024 * 1024 * 1024));
            }
            else if (fileSize >= 1024 * 1024)
            {
                return string.Format("{0:####0.00} MB", ((Double)fileSize) / (1024 * 1024));
            }
            else if (fileSize >= 1024)
            {
                return string.Format("{0:####0.00} KB", ((Double)fileSize) / 1024);
            }
            else
            {
                return string.Format("{0} bytes", fileSize);
            }

        }


        /// <summary>
        /// 获取时间格式
        /// 00:00:00时间格式
        /// </summary>
        /// <returns></returns>
        static string FormatTime(int hours = 0, int minutes = 0, int seconds = 0)
        {
            TimeSpan ts = new TimeSpan(hours, minutes, seconds);
            string hourStr = ts.Hours.ToString().PadLeft(2, '0');
            string minuteStr = ts.Minutes.ToString().PadLeft(2, '0');
            string secondStr = ts.Seconds.ToString().PadLeft(2, '0');
            return $"{hourStr}:{minuteStr}:{secondStr}";
        }

        /// <summary>
        /// 输出带刻录文件信息
        /// </summary>
        /// <param name="recorder"></param>
        static void ShowBurnMediaListInfo(Recorder recorder)
        {
            Console.WriteLine();
            Console.WriteLine($"待刻录媒体对象:{recorder.BurnMediaList.Count}个.总大小:{FormatFileSize(recorder.BurnMediaFileSize)}");
            Console.WriteLine($"待刻录媒体对象列表如下:");
            for (int i = 0; i < recorder.BurnMediaList.Count; i++)
            {
                Console.WriteLine($"{recorder.BurnMediaList[i].MediaName} " +
                    $"{FormatFileSize(recorder.BurnMediaList[i].Size)} " +
                    $"{recorder.BurnMediaList[i].MediaPath}");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// 输出刻录进度通知
        /// </summary>
        /// <param name="recorder"></param>
        static void ShowBurnProgressChanged(Recorder recorder)
        {
            Console.Clear();

            #region 搭建输出显示框架
            Console.WriteLine();
            Console.WriteLine($"**********************刻录中,请稍候**********************");
            Console.WriteLine();
            Console.WriteLine("  当前操作:"); //第4行当前操作
            Console.WriteLine();

            // 第6行绘制进度条背景
            Console.Write("  ");
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            for (int i = 1; i <= 50; i++)
            {   //设置50*1的为总进度
                Console.Write(" ");
            }

            Console.WriteLine();
            Console.BackgroundColor = colorBack;

            Console.WriteLine();    //第7行输出空行
            Console.WriteLine();    //第8行输出进度
            Console.WriteLine($"*********************************************************");  //第9行
            Console.WriteLine();    //第10行输出空行
            #endregion

            //进度变化通知时,更新相关行数据即可.
            recorder.OnBurnProgressChanged += (burnProgress) => {
                if (burnProgress.CurrentAction == 6)
                {   //刻录完成
                    Console.SetCursorPosition(0, 1);
                    Console.WriteLine($"*************************刻录完成************************");
                }
                //第4行 当前操作
                Console.SetCursorPosition(0, 3);
                Console.Write($"  当前操作:{burnProgress.CurrentActionName}");
                Console.Write("                  ");    //填充空白区域
                Console.ForegroundColor = colorFore;

                // 第6行 绘制进度条进度(进度条前预留2空格)
                Console.BackgroundColor = ConsoleColor.Yellow; // 设置进度条颜色
                Console.SetCursorPosition(2, 5); // 设置光标位置,参数为第几列和第几行
                for (int i = 0; i <burnProgress.Percent*50; i++)
                {   //每个整数写入1个空格
                    Console.Write(" "); // 移动进度条
                }
                Console.BackgroundColor = colorBack; // 恢复输出颜色
                
                //第8行 已用时间,总时间
                Console.ForegroundColor = ConsoleColor.Green;// 更新进度百分比,原理同上.
                Console.SetCursorPosition(0, 7);
                Console.Write($"  进度:{burnProgress.PercentStr}  " +
                    $"已用时间:{FormatTime(0, 0, burnProgress.ElapsedTime)}  " +
                    $"剩余时间:{FormatTime(0, 0, burnProgress.TotalTime - burnProgress.ElapsedTime)}");
                Console.Write("      ");    //填充空白区域
                Console.ForegroundColor = colorFore;

                Console.SetCursorPosition(0, 9);    //光标 定位到第10行
            };
        }
    }
}
