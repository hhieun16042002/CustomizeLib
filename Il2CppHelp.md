# Il2Cpp环境下实现原游戏的Interface
1.创建一个类，继承你想要实现的接口

2.实现接口里所有应实现的方法、字段、属性等，同时实现一个参数为IntPtr的构造函数

3.创建一个MonoBehaviour类，将其AddComponent至一个GameObject上，想要获取接口实例时，可以通过new YourCons(behaviour.Pointer)来获取，可以缓存这个对象