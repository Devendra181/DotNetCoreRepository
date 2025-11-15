

public class A
{
    private protected virtual void M1()
    {
       Console.WriteLine("A.M");
    }


    public void M2()
    {
        Console.WriteLine("A.M");
    }
    class B : A
    {
        public void M()
        {
            Console.WriteLine("B.M");
        }

        private protected override void M1()
        {
            Console.WriteLine("B.M");
        }

    }
}

public class C: A
{
    private int a = 1;
}


class Program
{
    public static void Main()
    {
       
        Console.ReadKey();
    }
}

