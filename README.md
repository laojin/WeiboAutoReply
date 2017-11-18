# 微博秒评论
由 https://github.com/huiyadanli/WeiboMonitor 项目的源码改版而来，原版权归源码所有，附加代码版权归本人所有，新版本遵循原版本的开源协议

## 界面
![界面](https://raw.githubusercontent.com/hebin123456/WeiboAutoReply/master/image/screenshot2.png)

## 版本更新
* 2017-11-15
* 修复某些dom页面解析时的问题

* 2017-11-16
* 新增按微博名字自动搜索微博ID/个性域名，如果搜索不准确，可以使用网页自行获取

* 2017-11-18
* 新增表情回复，让你的回复更精彩

# 微博秒赞器
微博自动点赞，监控微博页面，新发微博秒赞。

貌似用在女神身上不错（暴露我内心的想法了）

## 界面
![界面](https://raw.githubusercontent.com/huiyadanli/WeiboMonitor/master/image/screenshot0.png)

## 使用方法
* [账号] [密码] 处输入你的微博账号密码；
* [页面UID] 里面输入微博主页地址 `weibo.com/u/` 或者 `weibo.com/` 后面跟随的那一串字符，比如`http://weibo.com/booknsong?refer_flag=1028035010_&is_hot=1`中，`booknsong` 就是UID。**请确保`http://weibo.com/[UID]`是能够打开的**。
* [刷新间隔] GET 该微博页面地址的时间间隔（单位：秒），推荐 20s~60s 就能达到秒赞的效果。注意不要太小，时间久了可能会出现账号异常。
* [休息时间] 设置哪个时间段停止监控的，默认凌晨 2~6 点停止刷新，持续刷新新浪貌似会BAN IP。
* [强制验证码] 账号异常时需要选择强制验证码才能进行正常登陆。

## 相关说明
* 模拟登录这一块就是在我原来写的[C# 实现新浪微博模拟登录](https://github.com/huiyadanli/SinaLogin)上修改了一下。
* 实际测试时，不影响网页端微博的使用。
* 这个软件还未经过详细测试，还存在许多bug。

## 已知 BUG
* 把第一个页面上所有微博都给点赞了，暂时没有什么好的解决方法，无法判断第一次取得的微博列表是否正确。

## TODO
* 加入同时监控多个微博的功能（感觉意义不是很大，同时监控太多会影响电脑的正常使用）
* 界面修改（现在还是一幅测试功能的样子）
* cookie 时效问题的解决。
* 连续出错自动重新登陆。
* 准备推出 python 版本，无界面。（希望有生之年能推出来）

## License
WTFPL

HEBIN