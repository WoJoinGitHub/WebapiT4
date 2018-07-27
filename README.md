# 项目概况
使用T4生成的前后端分离的web框架，是我们无需编码就能实现一个带有基本功能的管理项目。
项目分为 后端asp.net webapi 和前端 vue单页面应用
## 后端 webapi的计划为：
  > * 1.根据数据库通过T4生成通用的 业务逻辑和webapi 特殊业务用分部类实现
  > * 2.根据DTO 生成通用的业务逻辑和webapi 特殊业务用分部类实现
  现阶段完成 第一部分
## 前端 Vue 单页面应用计划为：
   > * 1.通过 webapi生成 对应的 管理页面(增删改查） 特殊逻辑 通过'分部'代码实现
尚未完全实现，已经完成 from表单的数据驱动 table使用iview组件
   ### '分部'代码
   什么是'分部'代码？ 是我参考部分类想到的一个类似其思想的 实现方式
   具体实现为：1.利用 vue 的数据驱动特性 利用T4生成各表单 表格的数据模型 ，此部分和对应的代码 作为组件来引用到对应的页面，
   特殊部分 通过覆盖数据模型来修改。如：
   ``` bash
   {
      model: 'name', // 对应字段
      label: '出险原因', // 对应的文字
      type: 'select', // 组件类型
      data: [], // 组件数据
      placeholder: '请输入' , // 组件提示文字
      hiddenitem: true // 是否隐藏
      }
 ```    
这是一个表单的其中一个字段，我们统一生成为input 标签 如果我们想要改为 select 标签 我们在vue的生命周期 created 或者其他事件中
修改 对应的数据，这样就能满足特性化定制。从而实现类型分部类的效果
对于其他特殊的业务 我们也可以通过组件去实现 基本不会修改到我们生成的代码，从而为我们省去很多时间。
## vue的特性
个人认为 vue类似框架有两大特性 组件化和数据驱动 这里能够充分利用它的这些特性。
数据驱动这块当我看到iview的table组件时，深受启发（其实react 风格的代码就是这样的，我还没时间学） 所以实现了 form的数据驱动写法
## 写在最后
我这里使用的后端框架和前端框架只用作为参考，各位可以参考这一思想去改造自己的前后框架。有兴趣的同学可以加我qq 一起探讨3035865281 
