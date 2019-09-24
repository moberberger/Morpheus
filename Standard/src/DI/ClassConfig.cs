using System;
using System.Collections.Generic;
using System.Text;

namespace Morpheus.DependencyInjection
{
    public enum ELifecycle
    {
        New,
        Singleton,
        Singleton_PerThread,
        Pooled,
    }

    /// <summary>
    /// This class will describe all DI metadata for a Type.
    /// </summary>
    public class ClassConfig
    {
        private readonly Type m_type;
        private readonly DI m_owner;

        private bool m_override = true;
        private ClassConfig m_overrideObject;

        /// <summary>
        /// Does not remove configured settings, but does re-ignore them in favor of the
        /// original override. TODO: Add bool "Clear Configured Settings".
        /// </summary>
        public void Clear() => m_override = true;


        /// <summary>
        /// Lifecycle config
        /// </summary>
        private Type m_componentType;
        private object m_singleton;
        private bool m_asSingleton;


        /// <summary>
        /// May only be created by internal DI classes.
        /// </summary>
        /// <param name="_type">Must be constructed with this information</param>
        /// <param name="_owner"></param>
        /// <param name="_override"></param>
        internal ClassConfig( Type _type, DI _owner, ClassConfig _override )
        {
            m_type = _type;
            m_owner = _owner;
            m_override = true;
            m_overrideObject = _override;
        }


        /// <summary>
        /// When "this" type is requested, use the provided Type to create a new object.
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        public ClassConfig Use( Type _type )
        {
            m_override = false;
            m_componentType = _type;
            m_asSingleton = false;
            return this;
        }

        /// <summary>
        /// Use an object as a singleton for the type's GetObject value.
        /// </summary>
        /// <param name="_singleton"></param>
        /// <returns></returns>
        public ClassConfig Use( object _singleton ) => AsSingleton( _singleton );

        /// <summary>
        /// When "this" type is requested, use the provided Type to create a new object.
        /// </summary>
        /// <returns></returns>
        public ClassConfig Use<T>() where T : class => Use( typeof( T ) );


        /// <summary>
        /// 
        /// </summary>
        public ClassConfig AsSingleton( object _singleton = null )
        {
            m_override = false;

            m_asSingleton = false;
            m_singleton = _singleton ?? GetObject();
            m_asSingleton = true;

            return this;
        }

        /// <summary>
        /// Return an object for this Type based on this configuration.
        /// </summary>
        /// <returns>an object for this Type based on this configuration.</returns>
        public object GetObject()
        {
            if (m_override)
                return m_overrideObject.GetObject();

            if (m_asSingleton)
                return m_singleton;
            else
                return Activator.CreateInstance( m_componentType ?? m_type );
        }
    }
}
