using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Morpheus
{
    /// <summary>
    /// This class is a base class. It allows an application to override this class providing
    /// decorated properties that correspond to specific nodes in an XML document, assumed to
    /// reside on disk.
    /// </summary>
    /// <remarks> These attributes work very nicely in conjunction with the DataGridView control
    /// and the <see cref="CSortableBindingList{T}"/> class.
    /// <code>
    /// TODO: Expend this example to include explanation of the /x: namespace shenanigans.
    /// 
    ///[AXPath( "/x:Project/x:PropertyGroup[not(@Condition)]/x:AssemblyName" )]
    ///public string AssemblyName
    ///{
    ///    get => GetNodeText( MethodBase.GetCurrentMethod() );
    ///    set => SetNodeText( MethodBase.GetCurrentMethod(), value );
    ///}
    ///
    ///[AXPath( "/x:Project/x:PropertyGroup[not(@Condition)]/x:RootNamespace" )]
    ///public string RootNamespace
    ///{
    ///    get => GetNodeText( MethodBase.GetCurrentMethod() );
    ///    set => SetNodeText( MethodBase.GetCurrentMethod(), value );
    ///}
    /// </code></remarks>
    public abstract class Xml2Object
    {
        /// <summary>
        /// The XML Document that contains the whole DOM for the data
        /// </summary>
        protected XmlDocument m_doc;

        /// <summary>
        /// Namespace manager. Namespaces are absolutely critical with XPath.
        /// </summary>
        protected XmlNamespaceManager m_nsmgr;

        /// <summary>
        /// The filename
        /// </summary>
        private readonly string m_filename;

        /// <summary>
        /// TRUE when the data has changed.
        /// </summary>
        /// <returns>TRUE when the data has changed.</returns>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// TRUE when there's something fishy with the data
        /// 
        /// TODO: When? What? How?
        /// </summary>
        public bool IsQuestionable { get; private set; }

        /// <summary>
        /// When
        /// </summary>
        public bool IsReadOnly { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Namespace { get; private set; }

        /// <summary>
        /// Tuple for linking an XPath string to an <see cref="XmlNode"/> .
        /// </summary>
        public class NodeInfo
        {
            /// <summary>
            /// The XPath string
            /// </summary>
            public string Xpath { get; set; }

            /// <summary>
            /// The Xml node
            /// </summary>
            public XmlNode Node { get; set; }
        }

        private readonly Dictionary<MethodBase, NodeInfo> m_lookupNode = new Dictionary<MethodBase, NodeInfo>();


        /// <summary>
        /// Construct with the name of the XML file and the Namespace string. This class will
        /// provide access to namespace-qualified nodes via the "x:" alias.
        /// </summary>
        /// <param name="_filename">The filename containing the XML</param>
        public Xml2Object( string _filename )
        {
            m_filename = _filename;
            LoadXmlDocument();
        }

        /// <summary>
        /// Throw away this XmlDocument and replace it with what is on disk.
        /// </summary>
        public virtual void UndoChanges() => LoadXmlDocument();

        /// <summary>
        /// Write the current XmlDocument to disk (the same filename that was used to read the
        /// file)
        /// </summary>
        public virtual void SaveToFile()
        {
            m_doc.Save( m_filename );
            IsDirty = false;
        }

        /// <summary>
        /// Re-check the ReadOnly flag. Likely important for Perforce integration.
        /// </summary>
        public virtual void ReReadReadOnly()
        {
            var fi = new FileInfo( m_filename );
            IsReadOnly = fi.IsReadOnly;
        }



        /// <summary>
        /// Load an XML document from a file
        /// </summary>
        protected virtual void LoadXmlDocument()
        {
            m_doc = new XmlDocument();
            m_nsmgr = new XmlNamespaceManager( m_doc.NameTable );

            m_doc.Load( m_filename );
            IsDirty = false;
            ReReadReadOnly();
        }

        /// <summary>
        /// Should be called by the subclass whenever the STRUCTURE (not the "InnerText" values)
        /// of the <see cref="XmlDocument"/> change.
        /// </summary>
        /// <param name="_makeDirty">Make this "dirty" at the same time</param>
        protected virtual void ClearNodeCache( bool _makeDirty )
        {
            m_lookupNode.Clear();
            if (_makeDirty)
                IsDirty = true;
        }

        /// <summary>
        /// Get an <see cref="XmlNode"/> using the decoration <see cref="AXPath"/> on the
        /// <see cref="MethodBase"/> . Returns the XPath string from the AXPath decoration.
        /// </summary>
        /// <remarks>
        /// This method DOES NOT catch exceptions. If the application references a non-existent
        /// node in the document, it will try to create it. If it can't, then an exception is
        /// probably going to be thrown.
        /// </remarks>
        /// <param name="_methodBase"></param>
        /// <param name="_xpath"></param>
        /// <returns></returns>
        protected virtual XmlNode GetNode( MethodBase _methodBase, out string _xpath )
        {
            if (!m_lookupNode.TryGetValue( _methodBase, out var nodeInfo ))
            {
                nodeInfo = new NodeInfo();
                var prop = _methodBase.GetPropertyInfo();
                nodeInfo.Xpath = _xpath = prop.GetSingleAttribute<AXPath>()?.XPath;
                nodeInfo.Node = m_doc.SelectSingleNode( nodeInfo.Xpath, m_nsmgr );

                if (nodeInfo.Node == null)
                {
                    var idx = _xpath.LastIndexOf( '/' );
                    var parent = _xpath.Substring( 0, idx );
                    var nodeName = _xpath.Substring( idx + 3 ); // account for the namespace qualifier x:

                    var pnode = m_doc.SelectSingleNode( parent, m_nsmgr );
                    if (pnode != null) // there was a parent
                    {
                        nodeInfo.Node = m_doc.CreateNode( XmlNodeType.Element, nodeName, Namespace );
                        pnode.AppendChild( nodeInfo.Node );
                    }
                }
                m_lookupNode[_methodBase] = nodeInfo;
            }

            _xpath = nodeInfo.Xpath;
            return nodeInfo.Node;
        }

        /// <summary>
        /// Get node text for
        /// </summary>
        /// <param name="_methodBase"></param>
        /// <returns></returns>
        protected virtual string GetNodeText( MethodBase _methodBase )
        {
            var node = GetNode( _methodBase, out var _ );
            return node?.InnerText ?? "";
        }

        /// <summary>
        /// Set node text
        /// </summary>
        /// <param name="_methodBase"></param>
        /// <param name="_text"></param>
        protected virtual void SetNodeText( MethodBase _methodBase, string _text )
        {
            var node = GetNode( _methodBase, out _ );
            if (node != null && node.InnerText != _text)
            {
                node.InnerText = _text;
                IsDirty = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_xpath"></param>
        /// <returns></returns>
        protected virtual bool RemoveNode( string _xpath )
        {
            var nodes = m_doc.SelectNodes( _xpath, m_nsmgr );
            var retval = false;

            foreach (XmlNode node in nodes)
            {
                if (node != null)
                {
                    retval = true;
                    node.ParentNode.RemoveChild( node );
                }
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_root"></param>
        /// <param name="_elementName"></param>
        /// <param name="_innerText"></param>
        protected virtual void AssureNode( XmlNode _root, string _elementName, string _innerText )
        {
            if (_root == null) return;

            var node = _root.SelectSingleNode( "x:" + _elementName, m_nsmgr );
            if (node == null)
            {
                node = m_doc.CreateNode( XmlNodeType.Element, _elementName, Namespace );
                _root.AppendChild( node );
            }
            node.InnerText = _innerText;
        }
    }
}
