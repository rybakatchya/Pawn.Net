#include "core.p"

forward public test();
forward public test_int(value);
forward public do_fib(n);
main()
{
    print(''hello worlds'');
}

public test()
{

}

public test_int(value)
{

}

public do_fib(n)
{
    new last = 0;
    new cur = 1;
    n = n - 1;
    while(n)
    {
        --n;
        new tmp = cur;
        cur = last + cur;
        last = tmp;
    }
    return cur;
}