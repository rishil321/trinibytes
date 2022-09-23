import {Component, OnInit} from '@angular/core';
import {FormBuilder, FormGroup, Validators} from '@angular/forms';

@Component({
  selector: 'app-create-blog-post',
  templateUrl: './create-blog-post.component.html',
  styleUrls: ['./create-blog-post.component.css']
})
export class CreateBlogPostComponent implements OnInit {
  createBlogPostForm!: FormGroup;
  uploadImageForm!: FormGroup;

  constructor(private fb: FormBuilder) {

  }

  ngOnInit(): void {
    this.createBlogPostForm = this.fb.group({
      newBlogPostTitle: ['BlogPostTitleTest', Validators.required],
      newBlogPostBody: ['BlogPostBodyTest', Validators.required]
    });
    this.uploadImageForm = this.fb.group({
      newBlogPostImage: ['', Validators.required]
    });
  }


  createBlogPostSubmit(form: FormGroup) {
    console.log('Valid?', form.valid); // true or false
    console.log('Title', form.value.newBlogPostTitle,);
    console.log('Body', form.value.newBlogPostBody,);
  }

  onFileChange(event: any) {
    if (event.target.files.length > 0) {
      const file = event.target.files[0];
      this.uploadImageForm.patchValue({
        newBlogPostImage: file
      });
    }
  }

  uploadImageSubmit(form: FormGroup) {
    console.log(form.value.newBlogPostImage);
  }

}
