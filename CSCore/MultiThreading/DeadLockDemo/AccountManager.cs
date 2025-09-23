using System;
using System.Threading;

namespace DeadLockDemo
{
    public class AccountManager
    {
        private Account FromAccount; //Resource
        private Account ToAccount; //Resource
        private double TransferAmount;

        public AccountManager(Account AccountFrom, Account AccountTo, double AmountTransfer)
        {
            FromAccount = AccountFrom;
            ToAccount = AccountTo;
            TransferAmount = AmountTransfer;
        }

        //Thread1
        //FromAccount: Account1001 (Resource1) -- Acquired a Lock
        //ToAccount: Account1002 (Resource2) --No Lock, Waiting

        //Thread2
        //FromAccount: Account1002 (Resource1) -- Acquired a Lock
        //ToAccount: Account1001 (Resource2) --No Lock, Waiting
        public void FundTransfer()
        {
            Console.WriteLine($"{Thread.CurrentThread.Name} trying to acquire lock on {FromAccount.ID}");
            lock (FromAccount)
            {
                Console.WriteLine($"{Thread.CurrentThread.Name} acquired lock on {FromAccount.ID}");
                Console.WriteLine($"{Thread.CurrentThread.Name} Doing Some work");
                Thread.Sleep(1000);
                Console.WriteLine($"{Thread.CurrentThread.Name} trying to acquire lock on {ToAccount.ID}");

                if (Monitor.TryEnter(ToAccount, 3000))
                {
                    Console.WriteLine($"{Thread.CurrentThread.Name} acquired lock on {ToAccount.ID}");
                    try
                    {
                        FromAccount.WithdrawMoney(TransferAmount);
                        ToAccount.DepositMoney(TransferAmount);
                        Console.WriteLine($"{Thread.CurrentThread.Name} Completed its Task");
                    }
                    finally
                    {
                        Monitor.Exit(ToAccount);
                    }
                }
                else
                {
                    Console.WriteLine($"{Thread.CurrentThread.Name} Unable to acquire lock on {ToAccount.ID}, So exiting.");
                }
            }
        }
    }
}
