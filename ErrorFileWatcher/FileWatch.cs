using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Security;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace ErrorFileWatcher
{
    public class FileWatcher
    {
        Common cmn; // 共通クラス

        // データベース関連情報取得
        List<string> myEMailTo = new List<string>();
        List<string> myEMailCc = new List<string>();
        List<string> myIRepoKTNM = new List<string>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FileWatcher()
        {
            OracleConnection cnn = null;

            cmn = new Common();                         // 共通クラス
            cmn.DbCd = new DBConfigData();              // データベース設定データ
            cmn.FsCd = new FSConfigData();              // 設定データ クラス
            cmn.SMTPCd = new SMTPConfigData();          // SMTP設定データ クラス
            cmn.BaseDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');          // 実行ファイルのあるディレクトリ
            cmn.DbConfFilePath = Path.Combine(cmn.BaseDir, Common.CONFIG_FILE_DB);      // データベース設定データは実行ファイルと同一ディレクトリ
            cmn.FsConfFilePath = Path.Combine(cmn.BaseDir, Common.CONFIG_FILE_FS);      // ファイルシステム設定データは実行ファイルと同一ディレクトリ
            cmn.MailConfFilePath = Path.Combine(cmn.BaseDir, Common.CONFIG_FILE_SMTP);  // SMTP設定データは実行ファイルと同一ディレクトリ
            cmn.Dba = new DBAccess();                           // データベース アクセス クラス
            cmn.Fa = new FileAccess();                          // ファイル アクセス クラス
            cmn.DbCd = cmn.Fa.ReserializeDBConfigFile(cmn);     // データベース設定ファイル逆シリアライズ
            cmn.FsCd = cmn.Fa.ReserializeFSConfigFile(cmn);     // ファイルシステム設定ファイル逆シリアライズ
            cmn.SMTPCd = cmn.Fa.ReserializeMailConfigFile(cmn); // メール送信者情報設定ファイル逆シリアライズ
            if (cmn.Dba.ConnectSchema(cmn, ref cnn))            // データベース接続
            {
                cmn.Cnn = cnn;
                // データベース関連情報取得
                var myDs = new DataSet();
                
                // データベースから送信先メールアドレスを取得
                cmn.Dba.GetEmailAddress(cmn, Common.EMAIL_ADRTYPE_TO, ref myDs);
                cmn.Dba.GetEmailAddress(cmn, Common.EMAIL_ADRTYPE_CC, ref myDs);
                myEMailTo = myDs.Tables["To"].AsEnumerable().Select(x => x.Field<string>("EMAIL")).ToList();
                myEMailCc = myDs.Tables["Cc"].AsEnumerable().Select(x => x.Field<string>("EMAIL")).ToList();

                // データベースから有効かつ運用中の工程名称を取得
                if (!cmn.Dba.GetIRepoKt(cmn, Common.KM1060_ACTIVE_ACTIVE, Common.KM1060_OPESTAT_OPARATED, ref myDs)) return;
                myIRepoKTNM = myDs.Tables[Common.TABLE_NAME_KM1060].AsEnumerable()
                    .Select(row => row.Field<string>("IREPOKTNM")).ToList();

                // データベースはクローズ
                cmn.Dba.CloseSchema(cmn.Cnn);
                cmn.Cnn = null;
            }

        }

        /// <summary>
        /// 製造実績受入エラー監視
        /// </summary>
        public void Watch()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            
            // サーバーのファイルシステムに接続
            cmn.Fa.ConnectServer(cmn);

            watcher.Path = @cmn.FsCd.RootPath;      // コピー元ファイルの保存先
            watcher.Filter = @cmn.FsCd.FileFilter;  // コピー対象ファイルのフィルター
            watcher.IncludeSubdirectories = false;
            watcher.NotifyFilter = NotifyFilters.FileName;
            watcher.Created += new FileSystemEventHandler(DoWork);
            watcher.EnableRaisingEvents = true;
            
        }

        /// <summary>
        /// 製造日報受入エラーファイルの生成を検出
        /// </summary>
        /// <param name="source">検出元</param>
        /// <param name="e">ファイル システム イベント</param>
        private void DoWork(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType.ToString() == "Created")
            {
                try
                {
                    // 対象工程のみコピー
                    if (myIRepoKTNM.Contains(e.Name.Split('_')[0]))
                    {
                        // メール送信
                        SendMail(e.Name);
                        Debug.WriteLine("CALL Send Mail" + e.Name);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception Message = " + ex.Message);
                }
            }
        }
        
        private void SendMail(string _file)
        {
            try
            {
                using (MailMessage msg = new MailMessage())
                {
                    msg.From = new MailAddress(cmn.SMTPCd.From); // 送信元("コーケン工業　システム管理者 <yoshiyuki-watanabe@koken-kogyo.co.jp>")
                    foreach (var to in myEMailTo)
                    {
                        msg.To.Add(new MailAddress(to));
                    }
                    foreach (var cc in myEMailCc)
                    {
                        msg.CC.Add(new MailAddress(cc));
                    }
                    msg.Subject = Common.MAIL_SUBJECT;
                    msg.Body = Common.MAIL_BODY_HEADER +
                        "\n" +
                        Common.MAIL_BODY_FOOTER;

                    // 自動受入ログの添付（実行中はファイルにアクセス出来ない為複写して添付）
                    // \\kemsvr2\d$\IREPOEXE\exe\batch.log
                    var jidoulog_source = @cmn.FsCd.ShareName + Common.JIDOU_LOG_PATH + "\\" + Common.JIDOU_LOG_FILE;
                    if (File.Exists(jidoulog_source))
                    {
                        var jidoulog_dest = @cmn.BaseDir + "\\" + Common.JIDOU_LOG_FILE;
                        File.Copy(jidoulog_source, jidoulog_dest, true);
                        Attachment attachjidou = new Attachment(jidoulog_dest);
                        msg.Attachments.Add(attachjidou);
                    }

                    // errorファイルを添付
                    Attachment attacherror = new Attachment(@cmn.FsCd.RootPath + "\\" + _file);
                    msg.Attachments.Add(attacherror);

                    SmtpClient sc = new SmtpClient();

                    sc.EnableSsl = true;
                    sc.UseDefaultCredentials = false;
                    sc.Credentials = new System.Net.NetworkCredential(cmn.SMTPCd.LoginUser, cmn.SMTPCd.LoginPass); //uX[HgKn{djM56=xc
                    sc.Host = cmn.SMTPCd.Server; // "140.227.104.69";
                    sc.Port = cmn.SMTPCd.Port; // 587;
                    sc.DeliveryMethod = SmtpDeliveryMethod.Network;

                    // SSL証明書チェックのコールバック設定
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);

                    // メール送信
                    sc.Send(msg);

                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        private static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true; // .NET Frameworkに対して「SSL証明書の使用は問題なし」
            else
            {
                // false の場合は「SSL証明書の使用は不可」
                // 何かしらのチェックを行いtrue / false を判定
                // このプログラムでは true を返却し、信頼されないSSL証明書の問題を回避
                return true;
            }
        }
    }

}
