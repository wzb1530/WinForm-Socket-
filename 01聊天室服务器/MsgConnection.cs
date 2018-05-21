using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

namespace _01聊天室服务器
{
    /// <summary>
    /// 通信管理类，负责处理与某个客户端通信的过程
    /// </summary>
    public class MsgConnection
    {
        //与某个客户端通信套接字
        Socket sokMsg = null;
        //通信线程
        Thread thrMsg = null;
        //创建一个委托对象， 在窗体显示消息的方法
        DGShowMsg dgShow = null;
        //创建一个关闭连接的方法
        DGCloseConn dgCloseConn = null;

        #region 1.0 构造函数
        public MsgConnection(Socket sokMsg, DGShowMsg dgShow, DGCloseConn dgCloseConn)
        {
            this.sokMsg = sokMsg;
            this.dgShow = dgShow;
            this.dgCloseConn = dgCloseConn;
            //创建通信线程，负责调用通信套接字，来接收客户端消息。
            thrMsg = new Thread(ReceiveMsg);
            thrMsg.IsBackground = true;
            thrMsg.Start(this.sokMsg);
        }
        #endregion

        bool isReceive = true;
        #region  2.0 接收客户端发送的消息
        void ReceiveMsg(object obj)
        {
            Socket sockMsg = obj as Socket;
            //3 通信套接字 监听客户端的消息,传输的是byte格式。
            //3.1 开辟了一个 1M 的空间，创建的消息缓存区，接收客户端的消息。
            byte[] arrMsg = new byte[1024 * 1024 * 1];
            try
            {
                while (isReceive)
                {
                    //注意：Receive也会阻断当前的线程。
                    //3.2 接收客户端的消息,并存入消息缓存区。
                    //并 返回 真实接收到的客户端数据的字节长度。
                    int realLength = sockMsg.Receive(arrMsg);
                    //3.3 将接收的消息转成字符串
                    string strMsg = System.Text.Encoding.UTF8.GetString(arrMsg, 0, realLength);
                    //3.4 将消息显示到文本框
                    dgShow(strMsg);
                }
            }
            catch (Exception ex)
            {
                //调用窗体类的关闭移除方法
                dgCloseConn(sokMsg.RemoteEndPoint.ToString());
                //显示消息
                dgShow("客户端断开连接！");
            }
        }
        #endregion

        #region 3.0 向客户端发送文本消息 + void Send(string msg)
        /// <summary>
        /// 3.0 向客户端发送文本消息
        /// </summary>
        /// <param name="msg"></param>
        public void Send(string msg)
        {
            byte[] arrMsg = System.Text.Encoding.UTF8.GetBytes(msg);
            //通过指定的套接字将字符串发送到指定的客户端
            try
            {
                sokMsg.Send(MakeNewByte("str",arrMsg));
            }
            catch (Exception ex)
            {
                dgShow("异常" + ex.Message);
            }
        }
        #endregion

        #region 4.0 向客户端发送文件 + void SendFile(string strPath)
        /// <summary>
        /// 4.0 向客户端发送文件
        /// </summary>
        /// <param name="strFilePath"></param>
        public void SendFile(string strFilePath)
        {
            //4.1 读取要发送的文件
            byte[] arrFile = System.IO.File.ReadAllBytes(strFilePath);
            //4.2 向客户端发送文件
            sokMsg.Send(MakeNewByte("file", arrFile));
        }
        #endregion

        #region 4.1 向客户端发送抖屏命令 + void SendShake()
        /// <summary>
        /// 4.1 向客户端发送抖屏命令
        /// </summary>
        public void SendShake()
        {
            sokMsg.Send(new byte[1] { 2 });
        } 
        #endregion

        #region 5.0 返回带标识的新数组 + byte[] MakeNew(string type, byte[] oldArr)
        /// <summary>
        /// 返回带标识的新数组 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="oldArr"></param>
        /// <returns></returns>
        public byte[] MakeNewByte(string type, byte[] oldArr)
        {
            //5.1 创建一个新数组（是原数组长度 +1）
            byte[] newArrFile = new byte[oldArr.Length + 1];
            //5.2 将原数组数据复制到新数组中（从新数组下标为1的位置开始）
            oldArr.CopyTo(newArrFile, 1);
            //5.3 根据内容类型为新数组第一个元素设置标识符号
            switch (type.ToLower())
            {
                case "str":
                    newArrFile[0] = 0;  //只能存0-255之间的数值
                    break;
                case "file":
                    newArrFile[0] = 1;
                    break;
                default:
                    newArrFile[0] = 2;
                    break;
            }
            return newArrFile;
        }
        #endregion

        #region 6.0 关闭通信
        /// <summary>
        /// 关闭通信
        /// </summary>
        public void Close()
        {
            isReceive = false;
            sokMsg.Close();
            sokMsg = null;
        }
        #endregion
    }
}
