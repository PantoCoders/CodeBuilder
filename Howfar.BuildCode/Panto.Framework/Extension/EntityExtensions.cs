using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Panto.Framework.MVC;
namespace Panto.Framework
{
    /// <summary>
    /// 属性转换类，将一个类的属性值转换给另外一个类的同名属性，注意该类使用的是浅表复制。
    /// </summary>
    internal class ModuleCast
    {
        public List<CastProperty> mProperties = new List<CastProperty>();

        static Dictionary<Type, Dictionary<Type, ModuleCast>> mCasters = new Dictionary<Type, Dictionary<Type, ModuleCast>>(256);

        private static Dictionary<Type, ModuleCast> GetModuleCast(Type sourceType)
        {
            Dictionary<Type, ModuleCast> result;
            if (!mCasters.TryGetValue(sourceType, out result)) {
                lock (mCasters) {
                    if (!mCasters.TryGetValue(sourceType, out result)) {
                        result = new Dictionary<Type, ModuleCast>(1024);
                        mCasters.Add(sourceType, result);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取要转换的当前转换类实例
        /// </summary>
        /// <param name="sourceType">要转换的源类型</param>
        /// <param name="targetType">目标类型</param>
        /// <returns></returns>
        public static ModuleCast GetCast(Type sourceType, Type targetType)
        {
            Dictionary<Type, ModuleCast> casts = GetModuleCast(sourceType);
            ModuleCast result;
            if (!casts.TryGetValue(targetType, out result)) {
                lock (casts) {
                    if (!casts.TryGetValue(targetType, out result)) {
                        result = new ModuleCast(sourceType, targetType);
                        casts.Add(targetType, result);
                    }
                }
            }

            return result;
        }

        public static ModuleCast GetCast(Type targetType)
        {
            Dictionary<Type, ModuleCast> casts = GetModuleCast(targetType);
            ModuleCast result;
            if (!casts.TryGetValue(targetType, out result)) {
                lock (casts) {
                    if (!casts.TryGetValue(targetType, out result)) {
                        result = new ModuleCast(targetType);
                        casts.Add(targetType, result);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 以两个要转换的类型作为构造函数，构造一个对应的转换类
        /// </summary>
        /// <param name="targetType"></param>
        public ModuleCast(Type targetType)
        {
            PropertyInfo[] targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo tp in targetProperties) {
                CastProperty cp = new CastProperty();
                cp.TargetProperty = new PropertyAccessorHandler(tp);
                mProperties.Add(cp);
            }
        }


        /// <summary>
        /// 以两个要转换的类型作为构造函数，构造一个对应的转换类
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        public ModuleCast(Type sourceType, Type targetType)
        {
            PropertyInfo[] targetProperties = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo sp in sourceProperties) {
                foreach (PropertyInfo tp in targetProperties) {
                    if (sp.Name == tp.Name && sp.PropertyType == tp.PropertyType) {
                        CastProperty cp = new CastProperty();
                        cp.SourceProperty = new PropertyAccessorHandler(sp);
                        cp.TargetProperty = new PropertyAccessorHandler(tp);
                        mProperties.Add(cp);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 将源类型的属性值转换给目标类型同名的属性
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public void Cast(DataRow source, object target)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                throw new ArgumentNullException("target");

            mProperties.ForEach(t => {
                if (source.Table.Columns.Contains(t.TargetProperty.PropertyName)) {
                    object obj = source[t.TargetProperty.PropertyName];
                    //属性为guid类型,值为空时,obj的值为"{}"
                    if (obj != null && obj.GetType() != typeof(System.DBNull))
                    {
                        t.TargetProperty.Setter(target, Convert.ChangeType(obj, t.TargetProperty.PropertyType), null);
                    }
                }

            });
        }

        /// <summary>
        /// 将源类型的属性值转换给目标类型同名的属性
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public void Cast(object source, DataRow target)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                throw new ArgumentNullException("target");

            mProperties.ForEach(t => {
                if (target.Table.Columns.Contains(t.TargetProperty.PropertyName)) {

                    object obj = t.TargetProperty.Getter(source, null);

                    if (obj != null) {
                        target[t.TargetProperty.PropertyName] = obj;
                    }
                }
            });
        }


        /// <summary>
        /// 将源类型的属性值转换给目标类型同名的属性
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public void Cast(object source, object target)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null)
                throw new ArgumentNullException("target");

            for (int i = 0; i < mProperties.Count; i++) {
                CastProperty cp = mProperties[i];
                if (cp.SourceProperty.Getter != null) {
                    object Value = cp.SourceProperty.Getter(source, null);
                    if (cp.TargetProperty.Setter != null) {
                        cp.TargetProperty.Setter(target, Value, null);
                    }
                }
            }
        }

        /// <summary>
        /// 转换对象
        /// </summary>
        /// <typeparam name="TSource">源类型</typeparam>
        /// <typeparam name="TTarget">目标类型</typeparam>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象</param>
        public static void CastObject<TSource, TTarget>(TSource source, TTarget target)
            where TSource : class
            where TTarget : class
        {
            ModuleCast.GetCast(typeof(TSource), typeof(TTarget)).Cast(source, target);
        }


        /// <summary>
        /// 转换属性对象
        /// </summary>
        public class CastProperty
        {
            public PropertyAccessorHandler SourceProperty
            {
                get;
                set;
            }

            public PropertyAccessorHandler TargetProperty
            {
                get;
                set;
            }
        }

        /// <summary>
        /// 属性访问器
        /// </summary>
        public class PropertyAccessorHandler
        {
            public PropertyAccessorHandler(PropertyInfo propInfo)
            {
                this.PropertyName = propInfo.Name;

                if (propInfo.CanRead)
                    this.Getter = propInfo.GetValue;

                if (propInfo.CanWrite)
                    this.Setter = propInfo.SetValue;

                PropertyType = propInfo.PropertyType.GetRealType();
            }

            public Type PropertyType { get; set; }

            /// <summary>
            /// 字段名称
            /// </summary>
            public string PropertyName { get; set; }


            /// <summary>
            /// GET访问器
            /// </summary>
            public Func<object, object[], object> Getter { get; private set; }
            /// <summary>
            /// SET访问器
            /// </summary>
            public Action<object, object, object[]> Setter { get; private set; }
        }
    }

    /// <summary>
    /// 对象转换扩展
    /// </summary>
    public static class EntityExt
    {
        /// <summary>
        /// 将当前对象的属性值复制到目标对象
        /// </summary>
        /// <typeparam name="T">目标对象类型</typeparam>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象，如果为空，将生成一个</param>
        /// <param name="IsSerialize">是否深拷贝</param>
        /// <returns>复制过后的目标对象</returns>
        public static T FastCopy<T>(this object source, T target = default(T)) where T : new()
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (target == null) {
                target = Activator.CreateInstance<T>();
            }
            ModuleCast.GetCast(source.GetType(), typeof(T)).Cast(source, target);
            return target;
        }



        /// <summary>
        /// 将当前对象的属性值复制到目标对象
        /// </summary>
        /// <typeparam name="T">源对象类型</typeparam>
        /// <typeparam name="K">目标对象类型</typeparam>
        /// <param name="source">源对象</param>
        /// <param name="target">目标对象，如果为空，将生成一个</param>
        /// <returns>复制过后的目标对象</returns>
        public static List<K> FastCopys<T, K>(this List<T> source, List<K> target = null)
            where K : new()
            where T : new()
        {
            if (source != null && source.Count > 0) {

                if (target == null)
                    target = new List<K>();

                foreach (var item in source) {
                    target.Add(item.FastCopy<K>());
                }
            }
            return target;
        }

        /// <summary>
        /// DataTable转实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static List<T> ToEntity<T>(this DataTable source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            List<T> target = Activator.CreateInstance<List<T>>();


            foreach (DataRow dr in source.Rows) {
                T t = Activator.CreateInstance<T>();
                ModuleCast.GetCast(typeof(T)).Cast(dr, t);
                target.Add(t);
            }
            

            return target;
        }


        public static DataTable ToDataTable<T>(this List<T> list) {
           ModuleCast mc =  ModuleCast.GetCast(typeof(T));
            DataTable dt = new DataTable();

            for (int i = 0; i < mc.mProperties.Count; i++) {
                try {
                    dt.Columns.Add(mc.mProperties[i].TargetProperty.PropertyName, mc.mProperties[i].TargetProperty.PropertyType);
                }
                catch { }
            }

            foreach (T t in list) {
                DataRow dr = dt.NewRow();

                ModuleCast.GetCast(typeof(T)).Cast(t, dr);

                dt.Rows.Add(dr);
            }
            return dt;
        }
    }
}
