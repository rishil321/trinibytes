import {ComponentFixture, TestBed} from '@angular/core/testing';
import {ViewBlogPostComponent} from './view-blog-post.component';

describe('ViewBlogPostComponent', () => {
  let component: ViewBlogPostComponent;
  let fixture: ComponentFixture<ViewBlogPostComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ViewBlogPostComponent]
    })
      .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ViewBlogPostComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
