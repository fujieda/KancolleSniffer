using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Codeplex.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class SnifferTest
    {
        private StreamReader OpenLogFile(string path)
        {
            return new StreamReader(new GZipStream(File.Open(path, FileMode.Open), CompressionMode.Decompress));
        }

        private void SniffLogFile(Sniffer sniffer, string name)
        {
            var dir = Path.GetDirectoryName(Path.GetDirectoryName(Environment.CurrentDirectory));
            if (dir == null)
                return;
            var ln = 0;
            var stream = OpenLogFile(Path.Combine(dir, Path.Combine("logs", name + ".log.gz")));
            while (!stream.EndOfStream)
            {
                var triple = new List<string>();
                foreach (var s in new[] {"url: ", "request: ", "response: "})
                {
                    var line = stream.ReadLine();
                    ln++;
                    if (line == null)
                        throw new Exception(string.Format("ログのurl, request, responseがそろっていません: {0:d}行目", ln));
                    if (!line.StartsWith(s))
                        throw new Exception(string.Format("ログに不正な行が含まれています: {0:d}行目", ln));
                    triple.Add(line.Substring(s.Count()));
                }
                var json = DynamicJson.Parse(triple[2]);
                sniffer.Sniff(triple[0], triple[1], json);
            }
        }
    }
}