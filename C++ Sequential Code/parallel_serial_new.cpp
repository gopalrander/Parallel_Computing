#include<cstring>
#include <iostream>
#include <cstdio>
#include <cctype>
#include <string>
#include <cmath>
#include <vector>
#include <algorithm>
#include <stack>
#include <queue>
#include<climits>
#include<bitset>
#include <map>
#include <set>
#include <sstream>
#include <fstream>
#include <ctime>
#include <cassert>
 
using namespace std;

#define maxs 5  //...max no of channels per cell
#define maxc 7  //..max no of cell
#define maxr 1000 //..max no of request can be stored

//...shared data :: shared memory

//...neigbourhood matrix
int nhood[maxc][maxc];
int nh_s[maxc]; //..how many neighboring cells of i^th cell..
int block_cnt=0;

//universal id_serial number to be maintained
int uid=0;

  //...current time
  int t;  //substitute of universal time for each cell maintaining the 
          // integrity


struct cell_node
{
  // array of channel
  int channel[maxs];
  int placed[maxs];
  int burst[maxs];
  int r_ind[maxs];
  int c_cell[maxs];


  /// request to be sent
  int req_rec[maxr][5];//     <id> <from> <to> <burst> <done_bit>

  cell_node()
  {
    
    for(int i=0;i<maxs;i++)///initiallisation of the ARRAY_channel
      {
	channel[i]=-1;
	placed[i]=-1;
	burst[i]=-1;
	r_ind[i]=-1;
	c_cell[i]=-1;
      }
    
    t=0;//current time initialised to 0; for the sync in the universal time;
  
    for(int i=0;i<maxr;i++) //..initialisation of req_done bit 0 as all are done.
      {
	req_rec[i][4]=0;
      }
  }


}cell[maxc];



int main()
{
  //.....pre-computation of the neigbour_hood matrix:: adjacenecy list:

  for(int i=0;i<maxc;i++)
    {
      nh_s[i]=6;  //asumption: assumed the cell coverage to be hexagonal
      for(int j=0;j<6;j++)
	{
	  nhood[i][j]=(i+j+1)%maxc;
	}
    }

  //.................................

  int s,e,b;
  int id;

  srand( time(NULL) );


  for(int tt=0;tt<238;tt++)
    {
      //.....check for any call finished....

      for(int i=0;i<maxc;i++) //iterate over all cells... CUD_B_DONE_PARALLEL
	{
	  for(int j=0;j<maxs;j++)
	    {

	      if(cell[i].channel[j]!=-1&&(cell[i].placed[j]+cell[i].burst[j]<=t))
		{
		  cout<<"\n$%$deallocation:call with req_id: "<<cell[i].channel[j]<<" is done and channel_id: "<<j<<" of cell_id: "<<i<<" is de-allocated or freed:\n\n";
		  cell[i].channel[j]=-1;
		  cell[cell[i].c_cell[j]].req_rec[cell[i].r_ind[j]][4]=0;
		}

	    }

	}



      //...call made _request_to_call_made

      s=rand() % maxc;  // start..... 
      e=rand()% maxc;  //end......
      b=20+(rand()% 5) ;  // burst time....
      id=uid++;  //id given to the current request from universal id serial no

      ///...now the input is taken 



      /// reuquest recording and allocation ahead

      int temp=-1;
      for(int i=0;i<maxr;i++)
	{
	  if(cell[s].req_rec[i][4]==0)
	    {temp=i;break;}
	}
      // DONE = 0  <==> finished the job
      cell[s].req_rec[temp][0]=id; //...id
      cell[s].req_rec[temp][1]=s;  //...start
      cell[s].req_rec[temp][2]=e; //..end
      cell[s].req_rec[temp][3]=b; //..burst_time
      cell[s].req_rec[temp][4]=1; //..done_bit=1 i.e. (NOT DONE)

      //....request recorded..
  
      int ss=-1,es=-1;
      int sb=-1,eb=-1; //..check for borrow done.
      int stemp,etemp;
      int stemp2,etemp2;//for getting  donor_id
      int temp_s;//....for size of nhood_iterator
      //...now allocation to be done...
      for(int i=0;i<maxs;i++)///check 4 any channels (same) cell can be allocated 
	{
	  if(cell[s].channel[i]==-1)
	    {
	      stemp=i;ss=1;sb=-1;break;
	    }
	}

      ///.....if ss=1 here do the allocation here itself...


      if(ss==-1)  //...if not got from the channels same cell ..borrow
	{
	  temp_s=nh_s[s];//..gives the no. of neigbours of start(s)
	  for(int i=0;i<temp_s;i++)
	    {
	      for(int j=maxs-1;j>maxs/2;j--)
		{

		  if(cell[nhood[s][i]].channel[j]==-1)
		{ss=1;stemp2=i;stemp=j;sb=1;}

		}			    

	    }

	}



      for(int i=0;i<maxs;i++)///check 4 any channels (same) cell can be allocated 
	{
	  if(cell[e].channel[i]==-1)
	    {
	      etemp=i;es=1;break;
	    }
	}

      if(ss==1&&es==1)
	{
	  if(sb==-1) //....borrow is not done.....
      	    {
	      ///allocation of the sender channel
	      cell[s].channel[stemp]=id;
	      cell[s].placed[stemp]=t;
	      cell[s].burst[stemp]=b;
	      cell[s].r_ind[stemp]=temp;
	      cell[s].c_cell[stemp]=s;
	      cout<<"#1#allocation of req_id: "<<id<<" <requested by>"<<s<<" is done at time t: "<<t<<" cell_id: "<<s<<" and channel_id: "<<stemp<<"\n";
	    }
	  else//...borrow is done by sender
	    {
	      ///allocation of the sender channel
	      cell[stemp2].channel[stemp]=id;
	      cell[stemp2].placed[stemp]=t;
	      cell[stemp2].burst[stemp]=b;
	      cell[stemp2].r_ind[stemp]=temp;
	      cell[stemp2].c_cell[stemp]=stemp;
	      cout<<"#1#allocation of req_id: "<<id<<" <requested by>"<<s<<" <borrowed from>"<<stemp2<<" is done at time t: "<<t<<" cell_id: "<<stemp2<<" and channel_id: "<<stemp<<"\n";
	    }

	  //...allocation of end cell channel
	      cell[e].channel[etemp]=id;
	      cell[e].placed[etemp]=t;
	      cell[e].burst[etemp]=b;
	      cell[e].r_ind[etemp]=temp;
	      cell[e].c_cell[etemp]=stemp;
	      cout<<"#2#allocation of req_id: "<<id<<" <requested by>"<<s<<" <served by>"<<e<<" is done at time t: "<<t<<" cell_id: "<<e<<" and channel_id: "<<etemp<<"\n";



	}
      else
	{
	  cout<<"Blocked: call id"<<id<<"\n";
	  block_cnt++;
	}


      t+=2;///time is updated by units _lets_say_quanta

    }



  cout<<"\nTotal no. of blocks: "<<block_cnt<<"\n";





 return 0;
}
