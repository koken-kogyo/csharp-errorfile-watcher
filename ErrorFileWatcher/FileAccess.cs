using System.IO;
using System.Xml.Serialization;
using DecryptPassword;

namespace ErrorFileWatcher
{
    /// <summary>
    /// ファイル アクセス クラス
    /// </summary>
    class FileAccess
    {

        /// <summary>
        /// データベース設定ファイルの逆シリアライズ
        /// </summary>
        /// <param name="cmn">共通クラス</param>
        /// <returns></returns>
        public DBConfigData ReserializeDBConfigFile(Common cmn)
        {
            // 設定ファイル名
            string fileName = @cmn.DbConfFilePath;

            // XmlSerializerオブジェクトを作成
            XmlSerializer serializer = new XmlSerializer(typeof(DBConfigData));

            // 読み込むファイルを開く
            // 逆シリアライズ化
            using (StreamReader sr = new StreamReader(fileName, new System.Text.UTF8Encoding(false)))
            {
                // XMLファイルから読み込み、逆シリアル化する
                DBConfigData obj = (DBConfigData)serializer.Deserialize(sr);

                // ファイルを閉じる
                sr.Close();

                // パスワードをデコード
                if (obj.Passwd != null && obj.Passwd != "")
                {
                    DecryptPasswordClass decryptPasswordClass = new DecryptPasswordClass();
                    string cryptographyPassword = obj.Passwd;
                    string decryptionPassword;
                    if (decryptPasswordClass.DecryptPassword(cryptographyPassword, out decryptionPassword))
                    {
                        // デコードしたパスワードを格納
                        obj.Passwd = decryptionPassword;
                    }
                }
                return obj;
            }
        }

        /// <summary>
        /// ファイル システム設定ファイルの逆シリアライズ
        /// </summary>
        /// <param name="cmn">共通クラス</param>
        /// <returns></returns>
        public FSConfigData ReserializeFSConfigFile(Common cmn)
        {
            // 設定ファイル名
            string fileName = @cmn.FsConfFilePath;

            // XmlSerializer オブジェクトを作成
            XmlSerializer serializer = new XmlSerializer(typeof(FSConfigData));

            // 逆シリアライズ化
            using (StreamReader sr = new StreamReader(fileName, new System.Text.UTF8Encoding(false)))
            {
                // XMLファイルから読み込み、逆シリアル化する
                FSConfigData loadAry;
                loadAry = (FSConfigData)serializer.Deserialize(sr);

                // ファイルを閉じる
                sr.Close();

                // パスワードをデコード
                DecryptPasswordClass dp = new DecryptPasswordClass();
                if (loadAry.Passwd != null && loadAry.Passwd != "")
                {
                    string decryptionPassword;
                    if (dp.DecryptPassword(loadAry.Passwd, out decryptionPassword))
                    {
                        // デコードしたパスワードを格納
                        loadAry.Passwd = decryptionPassword;
                    }
                }
                return loadAry;
            }
        }

        /// <summary>
        /// メール関連設定ファイルの逆シリアライズ
        /// </summary>
        /// <param name="cmn">共通クラス</param>
        /// <returns></returns>
        /// [C#]階層化された繰返し要素のXMLをデシリアライズする
        /// https://zero-config.com/dotnet/xmlserializer002.html
        public SMTPConfigData ReserializeMailConfigFile(Common cmn)
        {
            // 設定ファイル名
            string fileName = @cmn.MailConfFilePath;

            // XmlSerializerオブジェクトを作成
            XmlSerializer serializer = new XmlSerializer(typeof(SMTPConfigData));

            // 読み込むファイルを開く
            // 逆シリアライズ化
            using (StreamReader sr = new StreamReader(fileName, new System.Text.UTF8Encoding(false)))
            {
                // XMLファイルから読み込み、逆シリアル化する
                SMTPConfigData obj = (SMTPConfigData)serializer.Deserialize(sr);

                // ファイルを閉じる
                sr.Close();

                // パスワードをデコード
                if (obj.LoginPass != null && obj.LoginPass != "")
                {
                    DecryptPasswordClass decryptPasswordClass = new DecryptPasswordClass();
                    string cryptographyPassword = obj.LoginPass;
                    string decryptionPassword;
                    if (decryptPasswordClass.DecryptPassword(cryptographyPassword, out decryptionPassword))
                    {
                        // デコードしたパスワードを格納
                        obj.LoginPass = decryptionPassword;
                    }
                }
                return obj;
            }
        }
    }
}
