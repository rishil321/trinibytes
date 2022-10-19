import {BrowserModule} from '@angular/platform-browser';
import {NgModule} from '@angular/core';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {HttpClientModule} from '@angular/common/http';
import {RouterModule} from '@angular/router';
import {AppComponent} from './app.component';
import {NavMenuComponent} from './nav-menu/nav-menu.component';
import {HomeComponent} from './home/home.component';
import {NgChartsModule} from "ng2-charts";
import {CaribbeanJobsPostsComponent} from './caribbeanjobsposts/caribbean-jobs-posts.component';
import {AboutMeComponent} from './about-me/about-me.component';
import {ViewBlogPostComponent} from './view-blog-post/view-blog-post.component';
import {CreateBlogPostComponent} from './create-blog-post/create-blog-post.component';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    CaribbeanJobsPostsComponent,
    AboutMeComponent,
    ViewBlogPostComponent,
    CreateBlogPostComponent
  ],
  imports: [
    BrowserModule.withServerTransition({appId: 'ng-cli-universal'}),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      {path: '', component: HomeComponent, pathMatch: 'full'},
      {path: 'create-blog-post', component: CreateBlogPostComponent},
      {path: 'caribbeanjobs-data', component: CaribbeanJobsPostsComponent},
      {path: 'about-me', component: AboutMeComponent},
    ]),
    ReactiveFormsModule,
    NgChartsModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {
}
