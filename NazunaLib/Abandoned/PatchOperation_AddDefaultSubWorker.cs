using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

namespace NareisLib
{
    // 代码版 Patch：为符合条件的 PawnRenderNodeProperties 追加 subworker
    public class PatchOperation_AddDefaultSubWorker : PatchOperation
    {
        // 必填：匹配哪些节点（建议精确到 renderTree 下的节点）
        public string xpath;

        // 要追加的 subworker 完整类型名（含命名空间）
        public string subworkerClass;

        // 可选：当 workerClass 是这个类型时跳过（完整类型名）
        public string skipWorkerClass;

        // 可选：如果列表里已包含这些类型名之一，就不追加
        public List<string> avoidIfExists;

        protected override bool ApplyWorker(XmlDocument xml)
        {
            bool changed = false;
            var nodes = xml.SelectNodes(xpath);
            if (nodes == null || nodes.Count == 0) return false;

            foreach (XmlNode node in nodes)
            {
                // 过滤 workerClass（支持属性或子元素两种写法）
                if (IsWorker(node, skipWorkerClass)) continue;

                // 确保有 <subworkerClasses>
                var list = node.SelectSingleNode("subworkerClasses");
                if (list == null)
                {
                    list = xml.CreateElement("subworkerClasses");
                    node.AppendChild(list);
                }

                // 如果已包含排除项或已包含目标，则跳过
                if (ContainsLi(list, subworkerClass)) continue;
                if (avoidIfExists != null && avoidIfExists.Any(a => ContainsLi(list, a))) continue;

                // 追加 <li>Fully.Qualified.TypeName</li>
                var li = xml.CreateElement("li");
                li.InnerText = subworkerClass;
                list.AppendChild(li);
                changed = true;
            }
            return changed;
        }

        private static bool IsWorker(XmlNode node, string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return false;
            // 属性形式：<li Class="...PawnRenderNodeProperties" workerClass="Full.Type"/>
            var attr = node.Attributes?["workerClass"]?.Value;
            if (attr == typeName) return true;
            // 子元素形式：
            var child = node.SelectSingleNode("workerClass");
            return child != null && child.InnerText == typeName;
        }

        private static bool ContainsLi(XmlNode list, string typeName)
        {
            if (list == null || string.IsNullOrEmpty(typeName)) return false;
            foreach (XmlNode li in list.ChildNodes)
                if (li.Name == "li" && li.InnerText == typeName) return true;
            return false;
        }
    }
}
