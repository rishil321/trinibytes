import {HttpClient} from '@angular/common/http';
import {Component, Inject, OnInit} from '@angular/core';
import {FormBuilder, FormGroup} from '@angular/forms';
import {ChartData, ChartOptions} from 'chart.js';
import {interval} from 'rxjs';
import {takeWhile} from 'rxjs/operators';

@Component({
  selector: 'app-caribbeanjobsposts',
  templateUrl: './caribbean-jobs-posts.component.html',
  styleUrls: ['./caribbean-jobs-posts.component.css']
})
export class CaribbeanJobsPostsComponent implements OnInit {
  private url!: string;
  jobPostsBeingFetched: boolean;

  constructor(private http: HttpClient, private fb: FormBuilder, @Inject('BASE_URL') baseUrl: string) {
    this.url = baseUrl + 'caribbeanjobsposts';
    this.jobPostsBeingFetched = true;
  }

  ngOnInit(): void {
    interval(5000)
      .pipe(takeWhile(() => true))
      .subscribe(() => {
        this.http.get(this.url + '/getlatestlogs')
          .subscribe({
            next: (result) => {
              let scrollTextDiv = document.getElementById("scrollTextDiv");
              if (scrollTextDiv != null && typeof result === "string") {
                scrollTextDiv.innerText = result;
                scrollTextDiv.scrollTop = scrollTextDiv.scrollHeight;
              }
            },
            error: (err) => {
              console.error(err);
            },
            complete: async () => {
              console.log("Fetch logs completed.");
            }
          });
        this.http.get(this.url + '/checkisscraperrunning')
          .subscribe({
            next: (result) => {
              if (result != null && typeof result === "boolean") {
                this.jobPostsBeingFetched = result;
              }
            },
            error: (err) => {
              console.error(err);
            },
            complete: async () => {
              console.log("checkisscraperrunning completed.");
            }
          });
      });
  }

  async startFetchCaribbeanJobsPosts() {
    this.jobPostsBeingFetched = true;
    console.log(this.jobPostsBeingFetched);
    this.http.post(this.url + '/startscrapecaribbeanjobsposts', {})
      .subscribe({
        next: (result) => {
          if (result == true) {
            console.log("Scraper started successfully.")
          } else {
            console.log("Failed to start scraper.")
          }
        },
        error: (err) => {
          console.error(err);
        },
        complete: async () => {
          console.log('startfetchcaribbeanjobsposts done.')
        }
      });
  }

  async stopFetchCaribbeanJobsPosts() {
    this.jobPostsBeingFetched = false;
    console.log(this.jobPostsBeingFetched);
    this.http.post(this.url + '/stopscrapecaribbeanjobsposts', {})
      .subscribe({
        next: (result) => {
          if (result == true) {
            console.log("Scraper stopped successfully.")
          } else {
            console.log("Scraper failed to be stopped.");
          }

        },
        error: (err) => {
          console.error(err);
        },
        complete: async () => {
          console.log('stopfetchcaribbeanjobsposts done.')
        }
      });
  }

}

function delay(milliseconds: number) {
  return new Promise(resolve => {
    setTimeout(resolve, milliseconds);
  });
}
