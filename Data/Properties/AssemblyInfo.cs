﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的常规信息通过以下
// 特性集控制。更改这些特性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("ViData")]
[assembly: AssemblyDescription("数据操作类库")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("DMedia")]
[assembly: AssemblyProduct("ViData")]
[assembly: AssemblyCopyright("Copyright ©  2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 使此程序集中的类型
// 对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型，
// 则将该类型上的 ComVisible 特性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("688fb435-4ce8-4087-b2c3-470119d2d38f")]

// 程序集的版本信息由下面四个值组成:
//
//      主版本
//      次版本 
//      内部版本号
//      修订号
//
// 可以指定所有这些值，也可以使用“内部版本号”和“修订号”的默认值，
// 方法是按如下所示使用“*”:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("2.4.15.124")]
[assembly: AssemblyFileVersion("2.4.15.124")]
//增加了分页方法的Count判定，优化了分页方法 2013-6-9
//增加了分页方法的BeginIndex, EndIndex参数 2013-7-15
//修复分页时当前页超过总页数无数据的问题 2013-8-5
//修复在MYSQL中的部分问题 2014-6-9
//新增数据库事务的手动控制 2015-1-24