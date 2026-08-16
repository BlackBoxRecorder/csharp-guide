---
title: 虚方法
description: C# 虚方法（Virtual Method）详解，包括虚方法的定义、重写、与抽象方法的区别、多态实现。
---

在C#中，虚方法（Virtual Method）提供了一个机制，允许在派生类中重写（Override）基类的方法。这意味着，当你有一个定义在基类中的方法，并且希望在派生类中对该方法进行特殊实现时，你可以将基类中的方法声明为虚方法，然后在派生类中使用`override`关键字来重写该方法。这种机制是实现多态性的关键，允许通过基类引用来调用派生类中的方法实现。

下面通过一个具体的代码示例来讲解虚方法的使用：

```csharp
using System;
// 定义一个基类
public class Animal
{
    // 声明一个虚方法
    public virtual void MakeSound()
    {
        Console.WriteLine("Some generic animal sound");
    }
}
// 定义一个派生类
public class Dog : Animal
{
    // 重写基类中的虚方法
    public override void MakeSound()
    {
        Console.WriteLine("Woof, woof!");
    }
}
// 定义另一个派生类
public class Cat : Animal
{
    // 重写基类中的虚方法
    public override void MakeSound()
    {
        Console.WriteLine("Meow, meow!");
    }
}
class Program
{
    static void Main()
    {
        // 创建基类和派生类的实例
        Animal myAnimal = new Animal();
        Animal myDog = new Dog();
        Animal myCat = new Cat();
        // 调用基类和派生类的方法
        myAnimal.MakeSound(); // 输出：Some generic animal sound
        myDog.MakeSound();    // 输出：Woof, woof!
        myCat.MakeSound();    // 输出：Meow, meow!
        // 使用基类引用调用派生类的方法，实现多态
        Animal[] animals = new Animal[] { myAnimal, myDog, myCat };
        foreach (Animal animal in animals)
        {
            animal.MakeSound();
        }
    }
}
```

在这个例子中，`Animal`类定义了一个虚方法`MakeSound`，该方法输出一个通用的动物声音。`Dog`和`Cat`类都继承自`Animal`类，并且重写了`MakeSound`方法以输出各自特定的声音。

在`Main`方法中，我们创建了`Animal`、`Dog`和`Cat`的实例。当我们调用`MakeSound`方法时，根据对象的实际类型，会调用相应的方法实现。这就是多态性的体现：一个基类的引用可以引用其派生类的对象，并且能够调用派生类中重写的方法。
