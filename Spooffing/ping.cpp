#include <stdio.h>
#include <sys/types.h>
#include <signal.h>
#include <unistd.h>
#include <sys/socket.h>
#include <netinet/ip_icmp.h>
#include <netinet/udp.h>
#include <netdb.h>
#include <sys/time.h>
#include <errno.h>

#define h_addr h_addr_list[0]
#define BUFSIZE 1000

struct sockaddr_in servaddr;
int sd;
//int nsent;
//int nreceived;
//int tsum = 0;
//int tmin = 1000;
//int tmax = 0;
in_addr_t src;
in_addr_t dst;
int packageSize;

void catcher(int sig)
{
	if (sig == SIGALRM)
 	{
 		pinger(); 
 		return;
 	} else if (sig == SIGINT) {
 		exit(-1);
 	}
}

void pinger()
{
	int icmplen;
	struct icmp *icmp;
	char sendbuf[BUFSIZE];
	struct iphdr *ip_hdr = (struct ip_hdr*) sendbuf;
	
	int ip_package_len = sizeof(struct iphdr) + sizeof(struct icmp) + packageSize;
	
	ip_hdr->ihl = 5;
	ip_hdr->version = 4;
	ip_hdr->tos = 0;
	ip_hdr->tot_len = htons(ip_package_len);
	ip_hdr->id = 0;
	ip_hdr->frag_off = 0;
	ip_hdr->ttl = 64;
	ip_hdr->protocol = IPPROTO_ICMP;
	ip_hdr->check = 0;
	ip_hdr->check = in_cksum((unsigned short *)ip_hdr, sizeof(struct iphdr));
	ip_hdr->saddr = src;
	ip_hdr->daddr = dst;

	icmp = (struct icmp *) sendbuf;
	icmp->icmp_type = ICMP_ECHO;
	icmp->icmp_code = 0;
	icmp->icmp_id = htons(1);
	icmp->icmp_seq = 1;
	icmp->icmp_cksum = 0;
	icmp->icmp_cksum = in_cksum((unsigned short*)icmp, sizeof(struct icmp) + packageSize);


	if(sendto(sd, sendbuf, ip_package_len, 0,
	
	 (struct sockaddr*)&servaddr, sizeof(servaddr)) < 0)
	{
		perror("sendto failed");
		exit(-1);
	}
}


unsigned short in_cksum(unsigned short *addr, int len) 
{
	unsigned short result;
 	unsigned int sum = 0;

	while (len > 1) 
	{
 		sum += *addr++;
 		len -= 2;
 	}
 	
 	if (len == 1)
 	sum += *(unsigned char*) addr;

	sum = (sum >> 16) + (sum & 0xFFFF);
 	sum += (sum >> 16);

 	result = ~sum; 
 	
 	return result;
}


int main(int argc, char* argv[])
{	
	if(argc < 4)
	{
		perror("\nUsage: ping [ip address or hostname src] [ip address or hostname destination] [package size].\n");
		exit(-1);
	}
	src = inet_addr(argv[1])
	dest = inet_addr(argv[2])
	packageSize = (int) argv[3];

	struct hostent* hp;
	bzero(&servaddr, sizeof(servaddr));
	servaddr.sin_family = AF_INET;
	servaddr.sen_addr.s_addr = src;

	struct sigaction act;

	memset(&act, 0, sizeof(act));
 	act.sa_handler = &catcher;
	
	sigaction(SIGALRM, &act, NULL);
	sigaction(SIGINT, &act, NULL);

	sd = socket(PF_INET, SOCK_RAW, IPPROTO_RAW);

	if(sd < 0)
	{
	 	perror("socket error.\n");
	 	exit(-1);
	}
	
	int size = 60 * 1024;
	int on = 1;

	setsockopt(sd, SOL_SOCKET, SO_RCVBUF, &size, sizeof(size));
	setsockopt(sd, IPPROTO_IP, IP_HDRINCL, (char*)&on, sizeof(on));
	
	//timer initializing
	struct itimerval timer;
	timer.it_value.tv_sec = 0;
	timer.it_value.tv_usec = 1;
	timer.it_interval.tv_sec = 1;
	timer.it_interval.tv_usec = 0;
	setitimer(ITIMER_REAL, &timer, NULL);

	while(1)
	{
		
	}

	close(sd);
	return 1;
}