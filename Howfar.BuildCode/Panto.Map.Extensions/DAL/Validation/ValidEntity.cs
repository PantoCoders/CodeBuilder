using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Panto.Map.Extensions.DAL;

namespace Panto.Map.Extensions.DAL
{
    /// <summary>
    /// 验证实体
    /// </summary>
    public class ValidEntity
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public ValidEntity()
        {
            this.Validations = new List<IValidation>();
        }
        /// <summary>
        /// 属性名
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 待验证的特性
        /// </summary>
        public List<IValidation> Validations { get; set; }
        /// <summary>
        /// GET访问器
        /// </summary>
        public Func<object, object[], object> Getter { get; set; }
        /// <summary>
        /// SET访问器
        /// </summary>
        public Action<object, object, object[]> Setter { get; set; }
    }
}
