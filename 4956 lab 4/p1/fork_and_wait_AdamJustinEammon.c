#include <sys/types.h>
#include <unistd.h>
#include <stdlib.h>
#include <stdio.h>
#include <time.h>

int delay(int input);

int main()
{
	int pid;
	int child_pid;
	int status;
	int exit_code;

	exit_code = 5;

	printf("I am the original process with PID %d and PPID %d (before fork) \n", getpid(), getppid());
	
	//Here, a child process is created with its own pid and a parent id of the current process
	pid = fork();

	if  (pid >0)
	{
		//Delays so that the child runs its code before the parent process
		delay(1);
		printf("I am the original process with PID %d and PPID %d (after fork )\n", getpid(), getppid() );
		printf("I received from OS my child pid: %d \n ", pid);
		//Waits until the child process has finished
		child_pid = wait(&status);
	} 
	else if (pid == 0)			/* I must be the child  because my parent got the return "0"*/			
	{
		printf ("I am the child process with PID %d and PPID %d \n", getpid(), getppid());
		printf("I am the child and with PID %d and PPID %d  and I will terminate before executing the parent's code\n", getpid(), getppid());
		exit(exit_code);
	}
	else
	{
		printf ("Error � no child process was created \n");
	}
	/* both processes execute the following instructions */
	printf ("PID %d After fork  was executed \n", getpid()); 
	printf ("PID %d Hello World \n", getpid()); 
	printf ("PID %d  terminates \n", getpid()); 	
	
	/* end of program */

	return 0;
}

int delay (int input)
{
	int ms = input * 1000;
	clock_t startTime = clock(); 
	while(clock() < startTime + ms){}
	return 0;
}