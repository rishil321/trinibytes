import {Component, Inject, OnInit} from '@angular/core';
import {HttpClient} from '@angular/common/http';

@Component({
  selector: 'app-view-blog-post',
  templateUrl: './view-blog-post.component.html',
  styleUrls: ['./view-blog-post.component.css']
})
export class ViewBlogPostComponent implements OnInit {
  retrievedBlogPosts: Array<BlogPost> | undefined;
  private url!: string;

  constructor(private http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.url = baseUrl + 'BlogPosts';
    this.http.get<Array<BlogPost>>(this.url + '/GetLatestBlogPosts').subscribe((Response: Array<BlogPost>) => {
      this.retrievedBlogPosts = Response;
    })
  }

  ngOnInit(): void {
  }

  get latestBlogPosts() {
    return (this.retrievedBlogPosts ? this.retrievedBlogPosts : null);
  }

}

class BlogPost {
  title: string | undefined;
  body: string | undefined;
  id: bigint | undefined;
  images: string | undefined;
  creationDate: Date | undefined;
  lastModifiedDate: Date | undefined;
}
