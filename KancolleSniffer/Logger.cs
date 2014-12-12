using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public class Logger
    {
        private Action<string, string, string> _writer;
        private Func<DateTime> _nowFunc;
        private const string DateTimeFormat = @"yyyy\-MM\-dd HH\:mm\:ss";

        public Logger()
        {
            _writer = new LogWriter().Write;
            _nowFunc = () => DateTime.Now;
        }

        public void SetWriter(Action<string, string, string> writer, Func<DateTime> nowFunc)
        {
            _writer = writer;
            _nowFunc = nowFunc;
        }

        public void InspectMissionResult(dynamic json)
        {
            var r = (int)json.api_clear_result;
            var rstr = r == 2 ? "大成功" : r == 1 ? "成功" : "失敗";
            var material = new int[7];
            if (r != 0)
                ((int[])json.api_get_material).CopyTo(material, 0);
            foreach (var i in new[] {1, 2})
            {
                var attr = "api_get_item" + i;
                if (!json.IsDefined(attr) || json[attr].api_useitem_id != -1)
                    continue;
                var count = (int)json[attr].api_useitem_count;
                var flag = ((int[])json.api_useitem_flag)[i - 1];
                if (flag == 1)
                    material[(int)Material.Bucket] = count;
                else if (flag == 2)
                    material[(int)Material.Burner] = count;
                else if (flag == 3)
                    material[(int)Material.Development] = count;
            }
            _writer("遠征報告書",
                string.Join(",", _nowFunc().ToString(DateTimeFormat),
                    rstr, json.api_quest_name, string.Join(",", material)),
                "日付,結果,遠征,燃料,弾薬,鋼材,ボーキ,開発資材,高速修復材,高速建造材");
        }

    }

    public class LogWriter
    {
        private readonly IFile _file;

        public interface IFile
        {
            string ReadAllText(string path);
            void AppendAllText(string path, string text);
            void Delete(string path);
            bool Exists(string path);
        }

        private class FileWrapper : IFile
        {
            // Shift_JISでないとExcelで文字化けする
            private readonly Encoding _encoding = Encoding.GetEncoding("Shift_JIS");

            public string ReadAllText(string path)
            {
                return File.ReadAllText(path, _encoding);
            }

            public void AppendAllText(string path, string text)
            {
                File.AppendAllText(path, text, _encoding);
            }

            public void Delete(string path)
            {
                File.Delete(path);
            }

            public bool Exists(string path)
            {
                return File.Exists(path);
            }
        }

        public LogWriter(IFile file = null)
        {
            _file = file ?? new FileWrapper();
        }

        public void Write(string file, string s, string header)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            var path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), file);
            var csv = path + ".csv";
            var tmp = path + ".tmp";
            if (_file.Exists(tmp))
            {
                try
                {
                    _file.AppendAllText(csv, _file.ReadAllText(tmp));
                    _file.Delete(tmp);
                }
                catch (IOException)
                {
                }
            }
            if (!_file.Exists(csv))
                s = header + "\r\n" + s;
            foreach (var f in new[] { csv, tmp })
            {
                try
                {
                    _file.AppendAllText(f, s + "\r\n");
                    break;
                }
                catch (IOException)
                {
                }
            }
        }
    }
}