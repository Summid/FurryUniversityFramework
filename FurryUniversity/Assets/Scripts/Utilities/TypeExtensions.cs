using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SFramework.Utilities
{
    public static class TypeExtensions
    {
        /// <summary>
        /// 获取程序集中某个类的所有子类
        /// </summary>
        /// <param name="self"></param>
        /// <param name="assemblyNameStartWith"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetSubTypesInAssemblies(this Type self)
        {
            return Assembly.GetAssembly(self).GetTypes()//获取该Type的程序集
                .Where(type => type.IsSubclassOf(self));//筛选
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

        /// <summary>
        /// 获取程序集中，实现了给定接口的子类
        /// </summary>
        /// <param name="selfInterface"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetInterfaceTypesInAssemblies(this Type selfInterface)
        {
            return Assembly.GetAssembly(selfInterface).GetTypes()
                .Where(type => type.GetInterface(selfInterface.FullName) != null);
        }
    }
}