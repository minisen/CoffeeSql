#### 介绍

CoffeeSql是一個輕量級的ORM框架，是本人在工作過程中積累並逐步完善的一個ORM框架，目前也在不斷地進行完善。
<br/>1、本ORM框架配備了與其他大多數ORM框架一樣的基本功能，可以映射表查詢數據到實體對象，映射的實現採用了效率更高的表達式樹緩存映射方法體           的方式，映射的速度更為快速。
<br/>2、可以使用lambda語法進行數據庫的增刪改查操作,可以進行一些簡單的sql語句的生成，減輕程序員頻繁手寫簡單sql語句的負擔，使開發更高效
<br/>3、具備一級緩存（sql語句查詢結果緩存）、二級緩存（表緩存）可以大大提高查詢的速度
<br/>4、提供sql執行打點日誌的切入點，使用者可以編寫日誌邏輯
<br/>5、支持讀寫分離（一主多從）的數據庫架構
<br/>6、提供實體的數據校驗功能，只需在實體類中進行簡單的Attribute配置即可
<br/>7、支持CodeFirst功能，可以由實體類生成建表DDL語句

#### 软件架构


![Coffee架构图示](https://images.gitee.com/uploads/images/2020/0112/164700_a3add843_1829372.png "Coffee.png")

#### 安装教程

下载项目代码后自行编译成dll文件引用到项目中

#### 使用说明

参考项目中的测试代码进行使用

#### 参与贡献

1.  Fork 本仓库
2.  新建 Feat_xxx 分支
3.  提交代码
4.  新建 Pull Request

#### 联系作者
QQ: 2822737354 (备注：码云 CoffeeSQL)


