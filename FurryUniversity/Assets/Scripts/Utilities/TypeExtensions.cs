using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Utilities
{
    public static class TypeExtensions
    {
        /// <summary>
        /// 获取程序集中某个类的所有子类
        /// </summary>
        /// <param name="self"></param>
        /// <param name="assemblyNameStartWith"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetSubTypesInAssemblies(this Type self, string assemblyNameStartWith = "Assembly")
        {
            return AppDomain.CurrentDomain.GetAssemblies()//获取所有程序集
                .Where(assembly => assembly.FullName.StartsWith(assemblyNameStartWith))//默认只提取Assembly开头的程序集，避免卡顿
                .SelectMany(assembly => assembly.GetTypes())//将每个程序集中所有Type对象提取出来
                .Where(type => type.IsSubclassOf(self));//获取self类型的子类
        }

        /// <summary>
        /// 获取程序集中某个类的子类，并且该子类被某个特性所标记
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetSubTypesWithClassAttributeInAssemblies<T>(this Type self) where T : Attribute
        {
            return GetSubTypesInAssemblies(self)
                .Where(type => type.GetCustomAttribute<T>() != null);
        }
    }
}